using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using PrintMaster.Models;
using PrintMaster.Services;

namespace PrintMaster.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IWorkflowStorageService _storage;
    private readonly IFileWatcherService _fileWatcher;
    private readonly IPrinterDiscoveryService _printerDiscovery;

    private ObservableCollection<PrintWorkflow> _workflows = new();
    private PrintWorkflow? _selectedWorkflow;
    private string _statusMessage = string.Empty;
    private ObservableCollection<string> _installedPrinters = new();

    public MainViewModel(
        IWorkflowStorageService storage,
        IFileWatcherService fileWatcher,
        IPrinterDiscoveryService printerDiscovery)
    {
        _storage = storage;
        _fileWatcher = fileWatcher;
        _printerDiscovery = printerDiscovery;

        DeleteWorkflowCommand = new RelayCommand(DeleteWorkflow);
        RefreshPrintersCommand = new RelayCommand(RefreshPrinters);

        LoadWorkflows();
        RefreshPrinters();
    }

    public ObservableCollection<PrintWorkflow> Workflows
    {
        get => _workflows;
        set => SetProperty(ref _workflows, value);
    }

    public PrintWorkflow? SelectedWorkflow
    {
        get => _selectedWorkflow;
        set => SetProperty(ref _selectedWorkflow, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value ?? string.Empty);
    }

    public ObservableCollection<string> InstalledPrinters
    {
        get => _installedPrinters;
        set => SetProperty(ref _installedPrinters, value);
    }

    public ICommand DeleteWorkflowCommand { get; }
    public ICommand RefreshPrintersCommand { get; }

    /// <summary>Workflow aus dem grafischen Editor übernehmen (Neu).</summary>
    public void AddWorkflowFromEditor(PrintWorkflow workflow)
    {
        if (workflow == null) return;
        Workflows.Add(workflow);
        _fileWatcher.StartWatching(workflow);
        Persist();
        StatusMessage = $"Workflow \"{workflow.Name}\" hinzugefügt.";
    }

    /// <summary>Workflow aus dem grafischen Editor übernehmen (Bearbeiten).</summary>
    public void UpdateWorkflowFromEditor(PrintWorkflow workflow)
    {
        if (workflow == null) return;
        var existing = Workflows.FirstOrDefault(w => w.Id == workflow.Id);
        if (existing == null) return;
        _fileWatcher.StopWatching(existing.Id);
        existing.Name = workflow.Name;
        existing.WatchPath = workflow.WatchPath;
        existing.FilePattern = workflow.FilePattern;
        existing.PrinterName = workflow.PrinterName;
        existing.DelaySeconds = workflow.DelaySeconds;
        existing.PostAction = workflow.PostAction;
        existing.MoveToPath = workflow.MoveToPath;
        existing.RenameTo = workflow.RenameTo;
        _fileWatcher.StartWatching(existing);
        Persist();
        StatusMessage = $"Workflow \"{existing.Name}\" aktualisiert.";
    }

    private void RefreshPrinters()
    {
        var list = _printerDiscovery.GetInstalledPrinterNames();
        InstalledPrinters.Clear();
        InstalledPrinters.Add("Testmodus");
        foreach (var p in list)
            InstalledPrinters.Add(p);
    }

    private void DeleteWorkflow()
    {
        if (SelectedWorkflow == null)
        {
            StatusMessage = "Bitte einen Workflow auswählen.";
            return;
        }

        _fileWatcher.StopWatching(SelectedWorkflow.Id);
        Workflows.Remove(SelectedWorkflow);
        SelectedWorkflow = null;
        Persist();
        StatusMessage = "Workflow gelöscht.";
    }

    private void LoadWorkflows()
    {
        var list = _storage.Load();
        Workflows.Clear();
        foreach (var w in list)
        {
            Workflows.Add(w);
            if (w.IsEnabled)
                _fileWatcher.StartWatching(w);
        }
    }

    private void Persist()
    {
        try
        {
            _storage.Save(Workflows);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Speichern fehlgeschlagen: {ex.Message}";
        }
    }

}
