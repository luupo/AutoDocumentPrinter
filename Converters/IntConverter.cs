using System.Globalization;
using System.Windows.Data;

namespace PrintMaster.Converters;

public class IntConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() ?? "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string s || !int.TryParse(s, out var i))
            return 1;
        return Math.Clamp(i, 0, 300);
    }
}
