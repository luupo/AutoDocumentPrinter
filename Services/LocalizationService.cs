using System.Globalization;
using System.IO;
using System.Text.Json;

namespace PrintMaster.Services;

public static class LocalizationService
{
    public const string SystemLanguage = "system";
    public const string English = "en";
    public const string German = "de";

    private static string LanguagePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PrintMaster", "language.json");

    private static System.Windows.ResourceDictionary? _languageDictionary;

    public static string GetSavedLanguage()
    {
        try
        {
            if (!File.Exists(LanguagePath)) return SystemLanguage;
            var json = File.ReadAllText(LanguagePath);
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("language", out var t))
            {
                var v = t.GetString();
                if (v == SystemLanguage || v == English || v == German) return v;
            }
        }
        catch { /* ignore */ }
        return SystemLanguage;
    }

    public static void SaveLanguage(string language)
    {
        try
        {
            var dir = Path.GetDirectoryName(LanguagePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(LanguagePath,
                JsonSerializer.Serialize(new { language }, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { /* ignore */ }
    }

    public static string GetEffectiveLanguage()
    {
        var saved = GetSavedLanguage();
        if (saved != SystemLanguage)
            return saved;

        var twoLetter = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
        return twoLetter == German ? German : English; // fallback to English
    }

    public static void ApplyLanguage(string? language = null)
    {
        language ??= GetEffectiveLanguage();
        if (language != English && language != German)
            language = English;

        try
        {
            var culture = new CultureInfo(language);
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
        }
        catch { /* ignore */ }

        var dict = LoadLanguageDictionary(language);
        if (dict == null) return;

        var app = global::System.Windows.Application.Current;
        if (app?.Resources?.MergedDictionaries == null) return;

        if (_languageDictionary != null)
        {
            app.Resources.MergedDictionaries.Remove(_languageDictionary);
            _languageDictionary = null;
        }
        app.Resources.MergedDictionaries.Insert(0, dict);
        _languageDictionary = dict;
    }

    public static string T(string key)
    {
        try
        {
            var app = global::System.Windows.Application.Current;
            if (app?.Resources.Contains(key) == true && app.Resources[key] is string s)
                return s;
        }
        catch { /* ignore */ }
        return key;
    }

    public static string F(string key, params object[] args)
        => string.Format(CultureInfo.CurrentUICulture, T(key), args);

    private static System.Windows.ResourceDictionary? LoadLanguageDictionary(string language)
    {
        var name = language == German ? "de.xaml" : "en.xaml";
        var uri = new Uri($"pack://application:,,,/PrintMaster;component/Languages/{name}", UriKind.Absolute);
        return new System.Windows.ResourceDictionary { Source = uri };
    }
}

