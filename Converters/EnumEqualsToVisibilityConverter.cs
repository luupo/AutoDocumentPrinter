using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PrintMaster.Converters;

public class EnumEqualsToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return Visibility.Collapsed;
        var paramStr = parameter.ToString();
        if (string.IsNullOrEmpty(paramStr)) return Visibility.Collapsed;
        return value.ToString()?.Equals(paramStr, StringComparison.OrdinalIgnoreCase) == true
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
