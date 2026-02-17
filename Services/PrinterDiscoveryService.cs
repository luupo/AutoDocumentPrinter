using Microsoft.Win32;

namespace PrintMaster.Services;

public class PrinterDiscoveryService : IPrinterDiscoveryService
{
    public IReadOnlyList<string> GetInstalledPrinterNames()
    {
        try
        {
            var list = new List<string>();
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Print\Printers");
            if (key == null)
                return list;

            foreach (var name in key.GetSubKeyNames())
            {
                if (!string.IsNullOrEmpty(name))
                    list.Add(name);
            }

            return list.OrderBy(x => x).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PrinterDiscoveryService: {ex.Message}");
            return new List<string>();
        }
    }
}
