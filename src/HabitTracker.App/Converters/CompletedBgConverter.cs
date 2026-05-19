using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace HabitTracker.App.Converters;

public class CompletedBgConverter : IValueConverter
{
    public static readonly CompletedBgConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool completed && completed)
            return new SolidColorBrush(Color.Parse("#E8F5E9"));

        return new SolidColorBrush(Color.Parse("#FAFAFA"));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}