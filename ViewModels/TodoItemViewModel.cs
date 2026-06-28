using Avalonia.Data.Converters;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using TodoApp.Models;

namespace TodoApp.ViewModels;

public partial class TodoItemViewModel : ObservableObject
{
    // Bootstrap pill colors
    public static readonly IValueConverter StatusBackgroundConverter = new StatusToColorConverter(s => s switch
    {
        TodoStatus.Creado => new SolidColorBrush(Color.Parse("#F8D7DA")),
        TodoStatus.EnEjecucion => new SolidColorBrush(Color.Parse("#FFF3CD")),
        TodoStatus.Terminado => new SolidColorBrush(Color.Parse("#D4EDDA")),
        _ => Brushes.Transparent
    });

    public static readonly IValueConverter StatusForegroundConverter = new StatusToColorConverter(s => s switch
    {
        TodoStatus.Creado => new SolidColorBrush(Color.Parse("#721C24")),
        TodoStatus.EnEjecucion => new SolidColorBrush(Color.Parse("#856404")),
        TodoStatus.Terminado => new SolidColorBrush(Color.Parse("#155724")),
        _ => Brushes.Black
    });

    public static readonly IValueConverter StatusBorderConverter = new StatusToColorConverter(s => s switch
    {
        TodoStatus.Creado => new SolidColorBrush(Color.Parse("#F5C6CB")),
        TodoStatus.EnEjecucion => new SolidColorBrush(Color.Parse("#FFEEBA")),
        TodoStatus.Terminado => new SolidColorBrush(Color.Parse("#C3E6CB")),
        _ => Brushes.Transparent
    });

    public int Id { get; set; }

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _text = string.Empty;

    [ObservableProperty]
    private TodoStatus _status;

    public int SortOrder { get; set; }

    public string StatusDisplay => Status switch
    {
        TodoStatus.Creado => "Creado",
        TodoStatus.EnEjecucion => "En ejecución",
        TodoStatus.Terminado => "Terminado",
        _ => Status.ToString()
    };

    partial void OnStatusChanged(TodoStatus value)
    {
        OnPropertyChanged(nameof(StatusDisplay));
    }

    public static TodoItemViewModel FromModel(TodoItem item) => new()
    {
        Id = item.Id,
        Title = item.Title,
        Text = item.Text,
        Status = item.Status,
        SortOrder = item.SortOrder
    };

    public TodoItem ToModel() => new()
    {
        Id = Id,
        Title = Title,
        Text = Text,
        Status = Status,
        SortOrder = SortOrder
    };
}
