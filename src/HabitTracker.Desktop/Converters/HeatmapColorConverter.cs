using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace HabitTracker.Desktop.Converters;

public class HeatmapColorConverter : IValueConverter
{
    public static readonly HeatmapColorConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int count)
            return new SolidColorBrush(Color.Parse("#EEEEEE"));

        return count switch
        {
            0 => new SolidColorBrush(Color.Parse("#EEEEEE")),    // серый — ничего не сделано
            1 => new SolidColorBrush(Color.Parse("#C8E6C9")),    // светло-зелёный
            2 => new SolidColorBrush(Color.Parse("#A5D6A7")),    // зелёный
            3 => new SolidColorBrush(Color.Parse("#66BB6A")),    // средне-зелёный
            4 => new SolidColorBrush(Color.Parse("#43A047")),    // насыщенный зелёный
            _ => new SolidColorBrush(Color.Parse("#2E7D32"))     // 5+ — тёмно-зелёный
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}