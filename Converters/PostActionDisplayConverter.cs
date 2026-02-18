using System.Globalization;
using System.Windows.Data;
using PrintMaster.Models;
using PrintMaster.Services;

namespace PrintMaster.Converters;

public class PostActionDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            PostActionType.None => LocalizationService.T("Loc_Post_None"),
            PostActionType.Delete => LocalizationService.T("Loc_Post_Delete"),
            PostActionType.Move => LocalizationService.T("Loc_Post_Move"),
            PostActionType.Rename => LocalizationService.T("Loc_Post_Rename"),
            _ => value?.ToString() ?? ""
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
