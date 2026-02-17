using System.IO;
using System.Text.Json;

namespace PrintMaster.Services;

public class AutostartService : IAutostartService
{
    private const string ShortcutName = "PrintMaster.lnk";
    private readonly string _settingsPath;
    private bool _cached;

    public AutostartService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dir = Path.Combine(appData, "PrintMaster");
        Directory.CreateDirectory(dir);
        _settingsPath = Path.Combine(dir, "settings.json");
        _cached = LoadFromFile();
    }

    public bool IsAutostartEnabled
    {
        get => _cached;
        private set
        {
            if (_cached == value) return;
            _cached = value;
            SaveToFile(value);
        }
    }

    public void SetAutostart(bool enable)
    {
        try
        {
            var startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            var shortcutPath = Path.Combine(startupFolder, ShortcutName);

            if (enable)
            {
                var exePath = Environment.ProcessPath ?? System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
                {
                    System.Diagnostics.Debug.WriteLine("AutostartService: Executable path not found.");
                    return;
                }
                CreateShortcut(shortcutPath, exePath);
            }
            else
            {
                if (File.Exists(shortcutPath))
                    File.Delete(shortcutPath);
            }

            IsAutostartEnabled = enable;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AutostartService: {ex.Message}");
        }
    }

    private static void CreateShortcut(string shortcutPath, string targetPath)
    {
        var shellType = Type.GetTypeFromProgID("WScript.Shell");
        if (shellType == null) return;
        var shell = Activator.CreateInstance(shellType);
        if (shell == null) return;

        var shortcut = shellType.InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { shortcutPath });
        if (shortcut == null) return;

        var st = shortcut.GetType();
        st.InvokeMember("TargetPath", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { targetPath });
        st.InvokeMember("WorkingDirectory", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { Path.GetDirectoryName(targetPath) ?? "" });
        st.InvokeMember("Save", System.Reflection.BindingFlags.InvokeMethod, null, shortcut, null);
    }

    private bool LoadFromFile()
    {
        try
        {
            if (!File.Exists(_settingsPath)) return false;
            var json = File.ReadAllText(_settingsPath);
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("autostart", out var prop) && prop.ValueKind == JsonValueKind.True)
                return true;
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), ShortcutName);
            return File.Exists(path);
        }
        catch
        {
            return false;
        }
    }

    private void SaveToFile(bool value)
    {
        try
        {
            var json = JsonSerializer.Serialize(new { autostart = value }, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AutostartService.SaveToFile: {ex.Message}");
        }
    }
}
