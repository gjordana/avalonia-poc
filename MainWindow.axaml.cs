using System;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Styling;
using TodoApp.Services;
using TodoApp.ViewModels;
using TodoApp.Views;

namespace TodoApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        SetWindowIcon();
        SetMacOsDockIcon();
        LoadContent();
    }

    private void SetWindowIcon()
    {
        var uri = new Uri("avares://TodoApp/Assets/avalonia-logo.png");
        using var stream = AssetLoader.Open(uri);
        Icon = new WindowIcon(stream);
    }

    private static void SetMacOsDockIcon()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return;

        try
        {
            var uri = new Uri("avares://TodoApp/Assets/avalonia-logo.png");
            using var stream = AssetLoader.Open(uri);
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            var pngBytes = ms.ToArray();

            var nsData = objc_msgSend_IntPtr(objc_getClass("NSData"),
                sel_registerName("dataWithBytes:length:"), pngBytes, pngBytes.Length);

            var nsImage = objc_msgSend_IntPtr(
                objc_msgSend_IntPtr(objc_getClass("NSImage"), sel_registerName("alloc")),
                sel_registerName("initWithData:"), nsData);

            var nsApp = objc_msgSend_IntPtr(objc_getClass("NSApplication"),
                sel_registerName("sharedApplication"));

            objc_msgSend_Void(nsApp, sel_registerName("setApplicationIconImage:"), nsImage);
        }
        catch
        {
            // Non-critical, ignore
        }
    }

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_getClass")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "sel_registerName")]
    private static extern IntPtr sel_registerName(string name);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector, byte[] arg1, int arg2);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_Void(IntPtr receiver, IntPtr selector, IntPtr arg1);

    private void OnThemeToggleChanged(object? sender, RoutedEventArgs e)
    {
        if (Application.Current == null) return;
        var isDark = ThemeToggle.IsChecked == true;
        Application.Current.RequestedThemeVariant = isDark ? ThemeVariant.Dark : ThemeVariant.Light;
    }

    private void LoadContent()
    {
        var config = ConfigService.Load();
        var dbPath = config.DatabasePath;
        var isValid = !string.IsNullOrEmpty(dbPath) &&
                      (File.Exists(dbPath) || Directory.Exists(Path.GetDirectoryName(dbPath)));

        if (!isValid)
        {
            ShowSetup();
        }
        else
        {
            ShowTodoList(dbPath!);
        }
    }

    private void ShowSetup()
    {
        var setupView = new SetupView();
        setupView.DatabaseConfigured += OnDatabaseConfigured;
        MainContent.Content = setupView;
    }

    private void OnDatabaseConfigured(string dbPath)
    {
        ShowTodoList(dbPath);
    }

    private void ShowTodoList(string dbPath)
    {
        var service = new TodoService(dbPath);
        var viewModel = new MainViewModel(service);
        var todoListView = new TodoListView { DataContext = viewModel };
        MainContent.Content = todoListView;
    }
}
