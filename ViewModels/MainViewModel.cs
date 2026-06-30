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
    public ObservableCollection<TodoItemViewModel> LeftTodos { get; } = new();
    public ObservableCollection<TodoItemViewModel> RightTodos { get; } = new();

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

    [ObservableProperty]
    private bool _isSplitView;

    private string _sortColumn = string.Empty;
    private bool _sortAscending = true;

    private string _leftSortColumn = string.Empty;
    private bool _leftSortAscending = true;
    private string _rightSortColumn = string.Empty;
    private bool _rightSortAscending = true;

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

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
        if (IsSplitView)
        {
            RefreshSplitSide(LeftTodos, _leftSortColumn, _leftSortAscending);
            RefreshSplitSide(RightTodos, _rightSortColumn, _rightSortAscending);
        }
    }

    partial void OnPageSizeChanged(int value)
    {
        CurrentPage = 1;
        ApplyFilter();
    }

    partial void OnIsSplitViewChanged(bool value)
    {
        if (value)
        {
            RefreshSplitSide(LeftTodos, _leftSortColumn, _leftSortAscending);
            RefreshSplitSide(RightTodos, _rightSortColumn, _rightSortAscending);
        }
    }

    // Single-grid sort (used by the main DataGrid)
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

    // Split-view sort — each side is independent
    public void SortSplitGrid(bool isLeft, string column)
    {
        if (isLeft)
        {
            if (_leftSortColumn == column) _leftSortAscending = !_leftSortAscending;
            else { _leftSortColumn = column; _leftSortAscending = true; }
            RefreshSplitSide(LeftTodos, _leftSortColumn, _leftSortAscending);
            OnPropertyChanged(nameof(LeftTitleHeader));
            OnPropertyChanged(nameof(LeftTextHeader));
            OnPropertyChanged(nameof(LeftStatusHeader));
        }
        else
        {
            if (_rightSortColumn == column) _rightSortAscending = !_rightSortAscending;
            else { _rightSortColumn = column; _rightSortAscending = true; }
            RefreshSplitSide(RightTodos, _rightSortColumn, _rightSortAscending);
            OnPropertyChanged(nameof(RightTitleHeader));
            OnPropertyChanged(nameof(RightTextHeader));
            OnPropertyChanged(nameof(RightStatusHeader));
        }
    }

    private void RefreshSplitSide(ObservableCollection<TodoItemViewModel> target, string sortColumn, bool ascending)
    {
        var sorted = sortColumn switch
        {
            "Title"  => ascending ? GetFilteredAll().OrderBy(t => t.Title)  : GetFilteredAll().OrderByDescending(t => t.Title),
            "Text"   => ascending ? GetFilteredAll().OrderBy(t => t.Text)   : GetFilteredAll().OrderByDescending(t => t.Text),
            "Status" => ascending ? GetFilteredAll().OrderBy(t => t.Status) : GetFilteredAll().OrderByDescending(t => t.Status),
            _        => GetFilteredAll().OrderBy(t => t.SortOrder)
        };
        target.Clear();
        foreach (var item in sorted) target.Add(item);
    }

    private IEnumerable<TodoItemViewModel> GetFilteredAll()
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
        return filtered;
    }

    public string GetSortIndicator(string column)
    {
        if (_sortColumn != column) return " ↕";
        return _sortAscending ? " ↑" : " ↓";
    }

    private string SplitIndicator(string sortColumn, bool ascending, string column)
    {
        if (sortColumn != column) return " ↕";
        return ascending ? " ↑" : " ↓";
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
            "Title"  => _sortAscending ? filtered.OrderBy(t => t.Title)  : filtered.OrderByDescending(t => t.Title),
            "Text"   => _sortAscending ? filtered.OrderBy(t => t.Text)   : filtered.OrderByDescending(t => t.Text),
            "Status" => _sortAscending ? filtered.OrderBy(t => t.Status) : filtered.OrderByDescending(t => t.Status),
            _        => filtered.OrderBy(t => t.SortOrder)
        };

        var filteredList = filtered.ToList();
        TotalPages = Math.Max(1, (int)Math.Ceiling(filteredList.Count / (double)PageSize));
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;

        var paged = filteredList.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
        Todos.Clear();
        foreach (var item in paged) Todos.Add(item);

        var from = filteredList.Count > 0 ? ((CurrentPage - 1) * PageSize) + 1 : 0;
        var to = Math.Min(CurrentPage * PageSize, filteredList.Count);
        PageInfo = $"Mostrando {from} a {to} de {filteredList.Count} registros";

        OnPropertyChanged(nameof(TitleHeader));
        OnPropertyChanged(nameof(TextHeader));
        OnPropertyChanged(nameof(StatusHeader));
    }

    // Single-grid headers
    public string TitleHeader  => "Título"      + GetSortIndicator("Title");
    public string TextHeader   => "Descripción" + GetSortIndicator("Text");
    public string StatusHeader => "Estado"      + GetSortIndicator("Status");

    // Split-view headers (independent per side)
    public string LeftTitleHeader  => "Título"      + SplitIndicator(_leftSortColumn,  _leftSortAscending,  "Title");
    public string LeftTextHeader   => "Descripción" + SplitIndicator(_leftSortColumn,  _leftSortAscending,  "Text");
    public string LeftStatusHeader => "Estado"      + SplitIndicator(_leftSortColumn,  _leftSortAscending,  "Status");
    public string RightTitleHeader  => "Título"      + SplitIndicator(_rightSortColumn, _rightSortAscending, "Title");
    public string RightTextHeader   => "Descripción" + SplitIndicator(_rightSortColumn, _rightSortAscending, "Text");
    public string RightStatusHeader => "Estado"      + SplitIndicator(_rightSortColumn, _rightSortAscending, "Status");

    public string CurrentSortColumn => _sortColumn;
    public bool IsSortAscending => _sortAscending;

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage > 1) { CurrentPage--; ApplyFilter(); }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage < TotalPages) { CurrentPage++; ApplyFilter(); }
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
                TodoStatus.Creado      => "Creado",
                TodoStatus.EnEjecucion => "En ejecución",
                TodoStatus.Terminado   => "Terminado",
                _                      => item.Status.ToString()
            };

            ws.Cell(i + 2, 3).Style.Fill.BackgroundColor = item.Status switch
            {
                TodoStatus.Creado      => XLColor.FromHtml("#F8D7DA"),
                TodoStatus.EnEjecucion => XLColor.FromHtml("#FFF3CD"),
                TodoStatus.Terminado   => XLColor.FromHtml("#D4EDDA"),
                _                      => XLColor.White
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
