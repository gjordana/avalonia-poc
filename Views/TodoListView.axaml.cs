using Avalonia.Controls;
using Avalonia.Interactivity;
using TodoApp.ViewModels;

namespace TodoApp.Views;

public partial class TodoListView : UserControl
{
    public TodoListView()
    {
        InitializeComponent();
    }

    private MainViewModel ViewModel => (MainViewModel)DataContext!;

    private async void OnNew(object? sender, RoutedEventArgs e)
    {
        var dialog = new EditTodoWindow();
        var topLevel = TopLevel.GetTopLevel(this) as Window;
        if (topLevel == null) return;

        var result = await dialog.ShowDialog<bool>(topLevel);
        if (result)
        {
            ViewModel.CreateTodo(dialog.TodoTitle, dialog.TodoText, dialog.TodoStatus);
            ViewModel.LoadTodos();
        }
    }

    private async void OnEdit(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: TodoItemViewModel todo }) return;

        var dialog = new EditTodoWindow(todo);
        var topLevel = TopLevel.GetTopLevel(this) as Window;
        if (topLevel == null) return;

        var result = await dialog.ShowDialog<bool>(topLevel);
        if (result)
        {
            todo.Title = dialog.TodoTitle;
            todo.Text = dialog.TodoText;
            todo.Status = dialog.TodoStatus;
            ViewModel.UpdateTodo(todo);
        }
    }

    private async void OnDelete(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: TodoItemViewModel todo }) return;

        var topLevel = TopLevel.GetTopLevel(this) as Window;
        if (topLevel == null) return;

        var confirm = new ConfirmDialog(
            "Confirmar eliminación",
            $"¿Está seguro que desea eliminar la tarea \"{todo.Title}\"?");

        var result = await confirm.ShowDialog<bool>(topLevel);
        if (result)
        {
            ViewModel.DeleteTodo(todo);
        }
    }
}
