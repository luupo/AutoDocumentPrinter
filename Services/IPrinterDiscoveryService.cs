namespace PrintMaster.Services;

public interface IPrinterDiscoveryService
{
    IReadOnlyList<string> GetInstalledPrinterNames();
}
