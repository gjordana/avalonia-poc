using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly TodoService _todoService;
    private List<TodoItemViewModel> _allTodos = new();

    public ObservableCollection<TodoItemViewModel> Todos { get; } = new();

    [ObservableProperty]
    private TodoItemViewModel? _selectedTodo;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int _pageSize = 25;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private string _pageInfo = string.Empty;

    [ObservableProperty]
    private string _alertMessage = string.Empty;

    [ObservableProperty]
    private bool _isAlertVisible;

    private string _sortColumn = string.Empty;
    private bool _sortAscending = true;

    public int[] PageSizeOptions { get; } = [10, 25, 50, 100];

    public MainViewModel(TodoService todoService)
    {
        _todoService = todoService;
        LoadTodos();
    }

    public void LoadTodos()
    {
        _allTodos = _todoService.GetAll().Select(TodoItemViewModel.FromModel).ToList();
        CurrentPage = 1;
        ApplyFilter();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnPageSizeChanged(int value)
    {
        CurrentPage = 1;
        ApplyFilter();
    }

    [RelayCommand]
    private void SortBy(string column)
    {
        if (_sortColumn == column)
            _sortAscending = !_sortAscending;
        else
        {
            _sortColumn = column;
            _sortAscending = true;
        }
        ApplyFilter();
    }

    public string GetSortIndicator(string column)
    {
        if (_sortColumn != column) return " ↕";
        return _sortAscending ? " ↑" : " ↓";
    }

    private void ApplyFilter()
    {
        var filtered = _allTodos.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.Trim();
            filtered = filtered.Where(t =>
                t.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                t.Text.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                t.StatusDisplay.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        filtered = _sortColumn switch
        {
            "Title" => _sortAscending ? filtered.OrderBy(t => t.Title) : filtered.OrderByDescending(t => t.Title),
            "Text" => _sortAscending ? filtered.OrderBy(t => t.Text) : filtered.OrderByDescending(t => t.Text),
            "Status" => _sortAscending ? filtered.OrderBy(t => t.Status) : filtered.OrderByDescending(t => t.Status),
            _ => filtered.OrderBy(t => t.SortOrder)
        };

        var filteredList = filtered.ToList();
        TotalPages = Math.Max(1, (int)Math.Ceiling(filteredList.Count / (double)PageSize));
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;

        var paged = filteredList
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        Todos.Clear();
        foreach (var item in paged)
            Todos.Add(item);

        var from = filteredList.Count > 0 ? ((CurrentPage - 1) * PageSize) + 1 : 0;
        var to = Math.Min(CurrentPage * PageSize, filteredList.Count);
        PageInfo = $"Mostrando {from} a {to} de {filteredList.Count} registros";

        OnPropertyChanged(nameof(TitleHeader));
        OnPropertyChanged(nameof(TextHeader));
        OnPropertyChanged(nameof(StatusHeader));
    }

    public string TitleHeader => "Título" + GetSortIndicator("Title");
    public string TextHeader => "Descripción" + GetSortIndicator("Text");
    public string StatusHeader => "Estado" + GetSortIndicator("Status");

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            ApplyFilter();
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            ApplyFilter();
        }
    }

    public void DeleteTodo(TodoItemViewModel todo)
    {
        _todoService.Delete(todo.Id);
        LoadTodos();
        ShowAlert("Tarea eliminada");
    }

    public TodoItem CreateTodo(string title, string text, TodoStatus status)
    {
        var item = _todoService.Add(title, text, status);
        ShowAlert("Tarea creada");
        return item;
    }

    public void UpdateTodo(TodoItemViewModel vm)
    {
        _todoService.Update(vm.ToModel());
        LoadTodos();
        ShowAlert("Tarea actualizada");
    }

    public string ExportToExcel(string filePath)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Tareas");

        ws.Cell(1, 1).Value = "Título";
        ws.Cell(1, 2).Value = "Descripción";
        ws.Cell(1, 3).Value = "Estado";

        var headerRange = ws.Range(1, 1, 1, 3);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#17A2B8");
        headerRange.Style.Font.FontColor = XLColor.White;

        var allItems = _todoService.GetAll();
        for (int i = 0; i < allItems.Count; i++)
        {
            var item = allItems[i];
            ws.Cell(i + 2, 1).Value = item.Title;
            ws.Cell(i + 2, 2).Value = item.Text;
            ws.Cell(i + 2, 3).Value = item.Status switch
            {
                TodoStatus.Creado => "Creado",
                TodoStatus.EnEjecucion => "En ejecución",
                TodoStatus.Terminado => "Terminado",
                _ => item.Status.ToString()
            };

            var statusCell = ws.Cell(i + 2, 3);
            statusCell.Style.Fill.BackgroundColor = item.Status switch
            {
                TodoStatus.Creado => XLColor.FromHtml("#F8D7DA"),
                TodoStatus.EnEjecucion => XLColor.FromHtml("#FFF3CD"),
                TodoStatus.Terminado => XLColor.FromHtml("#D4EDDA"),
                _ => XLColor.White
            };
        }

        ws.Columns().AdjustToContents();
        workbook.SaveAs(filePath);
        return filePath;
    }

    public void ShowAlert(string message)
    {
        AlertMessage = message;
        IsAlertVisible = true;
        Task.Delay(4000).ContinueWith(_ =>
            Dispatcher.UIThread.Post(() => IsAlertVisible = false));
    }
}
