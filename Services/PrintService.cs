using System.Diagnostics;
using System.IO;

namespace PrintMaster.Services;

public class PrintService : IPrintService
{
    public async Task<bool> PrintAsync(string filePath, string printerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;
        if (string.IsNullOrWhiteSpace(printerName))
            return false;
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
}
