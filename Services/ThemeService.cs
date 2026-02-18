using System.IO;
using System.Text.Json;
using System.Windows;

namespace PrintMaster.Services;

public static class ThemeService
{
    public const string Light = "light";
    public const string Dark = "dark";
    public const string System = "system";

    private static ResourceDictionary? _themeDictionary;

    public static string GetSavedTheme() => Light;

    public static void SaveTheme(string theme) { /* Darkmode deaktiviert – Einstellung ignorieren */ }

    public static string GetEffectiveTheme() => Light;

    public static void ApplyTheme(string? theme = null)
    {
        // Darkmode vorerst deaktiviert – immer Light-Theme laden
        var dict = LoadThemeDictionary(Light);
        if (dict == null) return;

        var app = global::System.Windows.Application.Current;
        if (app?.Resources?.MergedDictionaries == null) return;

        if (_themeDictionary != null)
        {
            app.Resources.MergedDictionaries.Remove(_themeDictionary);
            _themeDictionary = null;
        }
        app.Resources.MergedDictionaries.Insert(0, dict);
        _themeDictionary = dict;
    }

    private static ResourceDictionary? LoadThemeDictionary(string theme)
    {
        var name = theme == Dark ? "Dark.xaml" : "Light.xaml";
        var uri = new Uri($"pack://application:,,,/PrintMaster;component/Themes/{name}", UriKind.Absolute);
        return new ResourceDictionary { Source = uri };
    }
}
