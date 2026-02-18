using System.Diagnostics;
using System.IO;

namespace PrintMaster.Services;

public class PrintService : IPrintService
{
    private const string TestmodusPrinterNameDe = "Testmodus";
    private const string TestmodusPrinterNameEn = "Test mode";

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

                process.WaitForExit(TimeSpan.FromSeconds(30));
                return process.HasExited && process.ExitCode == 0;
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
