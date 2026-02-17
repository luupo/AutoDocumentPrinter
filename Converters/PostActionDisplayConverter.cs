using System.Globalization;
using System.Windows.Data;
using PrintMaster.Models;

namespace PrintMaster.Converters;

public class PostActionDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            PostActionType.None => "Keine Aktion",
            PostActionType.Delete => "Datei löschen",
            PostActionType.Move => "In Ordner verschieben",
            PostActionType.Rename => "Datei umbenennen",
            _ => value?.ToString() ?? ""
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
