using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
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
    }

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

    public void ShowAlert(string message)
    {
        AlertMessage = message;
        IsAlertVisible = true;
        Task.Delay(4000).ContinueWith(_ =>
            Dispatcher.UIThread.Post(() => IsAlertVisible = false));
    }
}
