using PrintMaster.Services;

namespace PrintMaster.ViewModels;

public static class ViewModelLocator
{
    private static readonly Lazy<ILogService> LogSvc = new(() => new LogService());
    private static readonly Lazy<MainViewModel> MainVm = new(CreateMainViewModel);

    public static MainViewModel Main => MainVm.Value;
    public static ILogService LogService => LogSvc.Value;

    private static MainViewModel CreateMainViewModel()
    {
        var storage = new WorkflowStorageService();
        var printService = new PrintService();
        var printerDiscovery = new PrinterDiscoveryService();
        var logService = LogSvc.Value;
        var fileWatcher = new FileWatcherService(printService, logService);
        return new MainViewModel(storage, fileWatcher, printerDiscovery);
    }
}
