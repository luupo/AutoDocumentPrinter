using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace PrintMaster.Services;

public class PrintService : IPrintService
{
    private const string TestmodusPrinterNameDe = "Testmodus";
    private const string TestmodusPrinterNameEn = "Test mode";
    private const int PrintProcessWaitSeconds = 25;

    public async Task<bool> PrintAsync(string filePath, string printerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;
        if (string.IsNullOrWhiteSpace(printerName))
            return false;

        if (IsTestPrinter(printerName))
        {
            await ShowTestmodusMessageAsync(filePath).ConfigureAwait(false);
            return true;
        }

        if (!File.Exists(filePath))
            return false;

        var ext = Path.GetExtension(filePath);
        var isPdf = string.Equals(ext, ".pdf", StringComparison.OrdinalIgnoreCase);
        var printStartedAt = DateTime.UtcNow;

        if (isPdf)
        {
            var result = await TryPrintPdfWithAcrobatAsync(filePath, printerName, cancellationToken).ConfigureAwait(false);
            if (result.HasValue)
            {
                if (result.Value)
                    await ClosePdfViewerProcessesStartedAfterAsync(printStartedAt).ConfigureAwait(false);
                return result.Value;
            }
        }

        var success = await PrintWithShellVerbAsync(filePath, printerName, cancellationToken).ConfigureAwait(false);
        if (isPdf && success)
            await ClosePdfViewerProcessesStartedAfterAsync(printStartedAt).ConfigureAwait(false);
        return success;
    }

    /// <summary>
    /// Schließt das Standard-PDF-Programm (und ggf. zugehörige Prozesse), wenn es nach dem angegebenen Zeitpunkt gestartet wurde – also durch unseren Druckauftrag.
    /// Welches Programm das ist, wird aus der Windows-Dateizuordnung für .pdf ausgelesen.
    /// </summary>
    private static readonly string[] FallbackPdfViewerProcessNames = { "AcroRd32", "Acrobat", "FoxitReader", "SUMATRAPDF", "PDFXCview", "msedge", "chrome", "firefox" };

    /// <summary>Prozessnamen, die nur Launcher sind – der echte Viewer ist ein anderer Prozess (z. B. UWP/Edge).</summary>
    private static readonly string[] KnownLauncherProcessNames = { "ApplicationFrameHost", "LaunchWinApp", "RuntimeBroker", "WinStore.App", "explorer" };

    private static async Task ClosePdfViewerProcessesStartedAfterAsync(DateTime utcAfter)
    {
        await Task.Delay(8000).ConfigureAwait(false); // Warten, bis Druckauftrag durch ist

        await Task.Run(() =>
        {
            var threshold = utcAfter.AddSeconds(-5); // Zeitfenster etwas breiter (Uhr/Verzögerung)
            var names = new List<string>(GetDefaultPdfViewerProcessNames());
            if (names.Count == 0 || names.Any(n => KnownLauncherProcessNames.Contains(n, StringComparer.OrdinalIgnoreCase)))
                names.AddRange(FallbackPdfViewerProcessNames);
            // Acrobat Reader lässt oft eine leere Instanz (Acrobat/AcroRd32) offen – beide Prozessnamen beenden
            if (names.Any(n => string.Equals(n, "AcroRd32", StringComparison.OrdinalIgnoreCase)) && !names.Any(n => string.Equals(n, "Acrobat", StringComparison.OrdinalIgnoreCase)))
                names.Add("Acrobat");
            names = names.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            foreach (var name in names)
            {
                try
                {
                    foreach (var p in Process.GetProcessesByName(name))
                    {
                        try
                        {
                            var started = p.StartTime.ToUniversalTime();
                            if (started >= threshold)
                                p.Kill(entireProcessTree: true);
                        }
                        catch (InvalidOperationException) { /* Prozess bereits beendet */ }
                        catch (System.ComponentModel.Win32Exception) { /* Rechte */ }
                        finally
                        {
                            p.Dispose();
                        }
                    }
                }
                catch { /* ignore */ }
            }
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Liest aus der Windows-Dateizuordnung das Standard-Programm für .pdf aus (shell\printto oder shell\open)
    /// und liefert dessen Prozessnamen (z. B. "AcroRd32", "msedge") zum gezielten Schließen.
    /// </summary>
    private static IReadOnlyList<string> GetDefaultPdfViewerProcessNames()
    {
        var list = new List<string>();
        try
        {
            using var pdfKey = Registry.ClassesRoot.OpenSubKey(".pdf");
            var progId = pdfKey?.GetValue("") as string;
            if (string.IsNullOrWhiteSpace(progId)) return list;

            // Zuerst printto (wird für Druck verwendet), sonst open
            var command = GetCommandForVerb(progId, "printto") ?? GetCommandForVerb(progId, "open");
            if (string.IsNullOrWhiteSpace(command)) return list;

            var exePath = ParseExePathFromCommand(command);
            if (string.IsNullOrWhiteSpace(exePath)) return list;

            var processName = Path.GetFileNameWithoutExtension(exePath);
            if (!string.IsNullOrWhiteSpace(processName))
                list.Add(processName);
        }
        catch { /* ignore */ }
        return list;
    }

    private static string? GetCommandForVerb(string progId, string verb)
    {
        try
        {
            using var shellKey = Registry.ClassesRoot.OpenSubKey($@"{progId}\shell\{verb}\command");
            return shellKey?.GetValue("") as string;
        }
        catch { return null; }
    }

    /// <summary>Extrahiert den ersten in Anführungszeichen stehenden Pfad aus einer Shell-Command-Zeile.</summary>
    private static string? ParseExePathFromCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command)) return null;
        var start = command.IndexOf('"');
        if (start < 0) return null;
        var end = command.IndexOf('"', start + 1);
        if (end < 0) return null;
        return command.Substring(start + 1, end - start - 1).Trim();
    }

    /// <summary>
    /// PDF per Adobe Reader /t drucken und Reader-Prozess danach beenden, damit kein Fenster offen bleibt.
    /// </summary>
    private static async Task<bool?> TryPrintPdfWithAcrobatAsync(string filePath, string printerName, CancellationToken cancellationToken)
    {
        var acrobatPath = GetAcrobatReaderPath();
        if (string.IsNullOrEmpty(acrobatPath) || !File.Exists(acrobatPath))
            return null;

        if (!TryGetPrinterDriverAndPort(printerName, out var driverName, out var portName))
            return null;

        return await Task.Run(() =>
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = acrobatPath,
                    Arguments = $"/t \"{filePath}\" \"{printerName}\" \"{driverName}\" \"{portName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process == null)
                    return false;

                var exited = process.WaitForExit(TimeSpan.FromSeconds(PrintProcessWaitSeconds));
                if (!exited)
                {
                    try { process.Kill(entireProcessTree: true); } catch { /* ignore */ }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintService (Acrobat /t): {ex.Message}");
                return false;
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    private static string? GetAcrobatReaderPath()
    {
        var exeName = "AcroRd32.exe";
        var paths = new[]
        {
            (Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Adobe\Acrobat Reader\DC")?.GetValue("InstallPath") as string) ?? "",
            (Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Adobe\Acrobat Reader\DC")?.GetValue("InstallPath") as string) ?? "",
            (Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Adobe\Acrobat Reader\2024")?.GetValue("InstallPath") as string) ?? "",
            (Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Adobe\Acrobat Reader\2024")?.GetValue("InstallPath") as string) ?? "",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Adobe", "Acrobat Reader DC", "Reader"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Adobe", "Acrobat Reader DC", "Reader")
        };

        foreach (var dir in paths.Distinct().Where(d => !string.IsNullOrWhiteSpace(d)))
        {
            var full = Path.Combine(dir.Trim(), exeName);
            if (File.Exists(full))
                return full;
        }

        return null;
    }

    private static bool TryGetPrinterDriverAndPort(string printerName, out string driverName, out string portName)
    {
        driverName = "";
        portName = "";
        var paths = new[]
        {
            $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Print\Printers\{printerName}",
            $@"SYSTEM\CurrentControlSet\Control\Print\Printers\{printerName}"
        };
        foreach (var path in paths)
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(path);
                if (key == null) continue;

                var d = key.GetValue("Driver") as string ?? "";
                var p = key.GetValue("Port") as string ?? "";
                if (string.IsNullOrEmpty(d) || string.IsNullOrEmpty(p)) continue;

                driverName = d.Trim();
                portName = p.Trim();
                return true;
            }
            catch { /* nächsten Pfad versuchen */ }
        }
        return false;
    }

    private static async Task<bool> PrintWithShellVerbAsync(string filePath, string printerName, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true,
                    Verb = "printto",
                    Arguments = $"\"{printerName}\"",
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process == null)
                    return false;

                // Kurz warten, bis der Auftrag an den Viewer übergeben wurde.
                // ExitCode 0 erwarten wir nicht – viele Viewer beenden sich nicht oder liefern ≠ 0.
                process.WaitForExit(TimeSpan.FromSeconds(5));
                return true; // Druck gestartet = Erfolg; Log „fehlgeschlagen“ nur bei echter Exception.
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintService: Win32Exception - {ex.Message}");
                return false;
            }
            catch (IOException ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintService: IOException (z.B. Datei gesperrt) - {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintService: {ex.Message}");
                return false;
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    private static async Task ShowTestmodusMessageAsync(string filePath)
    {
        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        if (dispatcher == null)
            return;
        await dispatcher.InvokeAsync(() =>
        {
            System.Windows.MessageBox.Show(
                LocalizationService.F("Loc_Test_Body", filePath),
                LocalizationService.T("Loc_Test_Title"),
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }).Task.ConfigureAwait(false);
    }

    private static bool IsTestPrinter(string printerName)
    {
        if (string.IsNullOrWhiteSpace(printerName)) return false;
        // Akzeptiere sowohl aktuellen (lokalisierten) Namen als auch die alte DE-Variante.
        var current = LocalizationService.T("Loc_TestPrinterName");
        return string.Equals(printerName, current, StringComparison.OrdinalIgnoreCase)
               || string.Equals(printerName, TestmodusPrinterNameDe, StringComparison.OrdinalIgnoreCase)
               || string.Equals(printerName, TestmodusPrinterNameEn, StringComparison.OrdinalIgnoreCase);
    }
}
