using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using TodoApp.Models;

namespace TodoApp.ViewModels;

public class StatusToColorConverter : IValueConverter
{
    private readonly Func<TodoStatus, IBrush> _selector;

    public StatusToColorConverter(Func<TodoStatus, IBrush> selector) => _selector = selector;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TodoStatus status)
            return _selector(status);
        return Brushes.Transparent;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
