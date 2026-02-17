using System.Diagnostics;
using System.IO;

namespace PrintMaster.Services;

public class PrintService : IPrintService
{
    private const string TestmodusPrinterName = "Testmodus";

    public async Task<bool> PrintAsync(string filePath, string printerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;
        if (string.IsNullOrWhiteSpace(printerName))
            return false;

        if (string.Equals(printerName, TestmodusPrinterName, StringComparison.OrdinalIgnoreCase))
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
                $"Der Workflow wurde ausgelöst.\n\nDatei: {filePath}\n\nEs ist noch der Testmodus eingestellt.",
                "PrintMaster – Testmodus",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }).Task.ConfigureAwait(false);
    }
}
