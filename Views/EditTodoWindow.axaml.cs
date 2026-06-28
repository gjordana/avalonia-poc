using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using TodoApp.Models;
using TodoApp.ViewModels;

namespace TodoApp.Views;

public partial class EditTodoWindow : Window
{
    private static readonly List<StatusOption> Statuses =
    [
        new("Creado", TodoStatus.Creado),
        new("En ejecución", TodoStatus.EnEjecucion),
        new("Terminado", TodoStatus.Terminado)
    ];

    public string TodoTitle => TitleBox.Text ?? string.Empty;
    public string TodoText => TextBox.Text ?? string.Empty;
    public TodoStatus TodoStatus => (StatusCombo.SelectedItem as StatusOption)?.Value ?? TodoStatus.Creado;

    public EditTodoWindow() : this(null) { }

    public EditTodoWindow(TodoItemViewModel? existing)
    {
        InitializeComponent();

        StatusCombo.ItemsSource = Statuses;
        StatusCombo.DisplayMemberBinding = new Avalonia.Data.Binding("Display");

        if (existing != null)
        {
            HeaderText.Text = "Edición de Tarea";
            Title = "Edición de Tarea";
            TitleBox.Text = existing.Title;
            TextBox.Text = existing.Text;
            StatusCombo.SelectedIndex = Statuses.FindIndex(s => s.Value == existing.Status);
        }
        else
        {
            HeaderText.Text = "Nueva Tarea";
            Title = "Nueva Tarea";
            StatusCombo.SelectedIndex = 0;
        }
    }

    private void OnSave(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleBox.Text))
        {
            TitleBox.Focus();
            return;
        }
        Close(true);
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private record StatusOption(string Display, TodoStatus Value)
    {
        public override string ToString() => Display;
    }
}
