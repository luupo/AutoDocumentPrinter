using PrintMaster.Services;

namespace PrintMaster.ViewModels;

public static class ViewModelLocator
{
    private static readonly Lazy<MainViewModel> MainVm = new(CreateMainViewModel);

    public static MainViewModel Main => MainVm.Value;

    private static MainViewModel CreateMainViewModel()
    {
        var storage = new WorkflowStorageService();
        var printService = new PrintService();
        var printerDiscovery = new PrinterDiscoveryService();
        var fileWatcher = new FileWatcherService(printService);
        return new MainViewModel(storage, fileWatcher, printerDiscovery);
    }
}
