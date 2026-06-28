using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using TodoApp.Services;

namespace TodoApp.Views;

public partial class SetupView : UserControl
{
    public event Action<string>? DatabaseConfigured;

    public SetupView()
    {
        InitializeComponent();
    }

    private async void OnSelectPath(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Seleccionar ubicación de la base de datos",
            SuggestedFileName = "todos.db",
            FileTypeChoices =
            [
                new FilePickerFileType("SQLite Database") { Patterns = ["*.db"] }
            ]
        });

        if (file != null)
        {
            PathTextBox.Text = file.Path.LocalPath;
            CreateButton.IsEnabled = true;
        }
    }

    private void OnCreate(object? sender, RoutedEventArgs e)
    {
        var path = PathTextBox.Text;
        if (string.IsNullOrWhiteSpace(path)) return;

        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var config = new AppConfig { DatabasePath = path };
        ConfigService.Save(config);

        DatabaseConfigured?.Invoke(path);
    }
}
