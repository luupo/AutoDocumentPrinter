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
    private string _newName = string.Empty;
    private string _newWatchPath = string.Empty;
    private string _newFilePattern = string.Empty;
    private string _newPrinterName = string.Empty;
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

        AddWorkflowCommand = new RelayCommand(AddWorkflow);
        EditWorkflowCommand = new RelayCommand(EditWorkflow);
        DeleteWorkflowCommand = new RelayCommand(DeleteWorkflow);
        PickFolderCommand = new RelayCommand(PickFolder);
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
        set
        {
            if (SetProperty(ref _selectedWorkflow, value) && value != null)
            {
                NewName = value.Name;
                NewWatchPath = value.WatchPath;
                NewFilePattern = value.FilePattern;
                NewPrinterName = value.PrinterName;
            }
        }
    }

    public string NewName
    {
        get => _newName;
        set => SetProperty(ref _newName, value ?? string.Empty);
    }

    public string NewWatchPath
    {
        get => _newWatchPath;
        set => SetProperty(ref _newWatchPath, value ?? string.Empty);
    }

    public string NewFilePattern
    {
        get => _newFilePattern;
        set => SetProperty(ref _newFilePattern, value ?? string.Empty);
    }

    public string NewPrinterName
    {
        get => _newPrinterName;
        set => SetProperty(ref _newPrinterName, value ?? string.Empty);
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

    public ICommand AddWorkflowCommand { get; }
    public ICommand EditWorkflowCommand { get; }
    public ICommand DeleteWorkflowCommand { get; }
    public ICommand PickFolderCommand { get; }
    public ICommand RefreshPrintersCommand { get; }

    private void AddWorkflow()
    {
        if (string.IsNullOrWhiteSpace(NewName))
        {
            StatusMessage = "Bitte einen Namen eingeben.";
            return;
        }
        if (string.IsNullOrWhiteSpace(NewWatchPath) || !Directory.Exists(NewWatchPath))
        {
            StatusMessage = "Bitte einen gültigen Ordner auswählen.";
            return;
        }
        if (string.IsNullOrWhiteSpace(NewPrinterName))
        {
            StatusMessage = "Bitte einen Drucker auswählen.";
            return;
        }

        var workflow = new PrintWorkflow
        {
            Name = NewName.Trim(),
            WatchPath = NewWatchPath.Trim(),
            FilePattern = string.IsNullOrWhiteSpace(NewFilePattern) ? "*.*" : NewFilePattern.Trim(),
            PrinterName = NewPrinterName
        };

        Workflows.Add(workflow);
        _fileWatcher.StartWatching(workflow);
        PersistAndClearForm();
        StatusMessage = $"Workflow \"{workflow.Name}\" hinzugefügt.";
    }

    private void EditWorkflow()
    {
        if (SelectedWorkflow == null)
        {
            StatusMessage = "Bitte einen Workflow auswählen.";
            return;
        }
        if (string.IsNullOrWhiteSpace(NewName))
        {
            StatusMessage = "Bitte einen Namen eingeben.";
            return;
        }
        if (string.IsNullOrWhiteSpace(NewWatchPath) || !Directory.Exists(NewWatchPath))
        {
            StatusMessage = "Bitte einen gültigen Ordner auswählen.";
            return;
        }
        if (string.IsNullOrWhiteSpace(NewPrinterName))
        {
            StatusMessage = "Bitte einen Drucker auswählen.";
            return;
        }

        _fileWatcher.StopWatching(SelectedWorkflow.Id);

        SelectedWorkflow.Name = NewName.Trim();
        SelectedWorkflow.WatchPath = NewWatchPath.Trim();
        SelectedWorkflow.FilePattern = string.IsNullOrWhiteSpace(NewFilePattern) ? "*.*" : NewFilePattern.Trim();
        SelectedWorkflow.PrinterName = NewPrinterName;

        _fileWatcher.StartWatching(SelectedWorkflow);
        Persist();
        StatusMessage = $"Workflow \"{SelectedWorkflow.Name}\" aktualisiert.";
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
        ClearForm();
        Persist();
        StatusMessage = "Workflow gelöscht.";
    }

    private void PickFolder()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Ordner für die Überwachung auswählen",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            NewWatchPath = dialog.SelectedPath;
    }

    private void RefreshPrinters()
    {
        var list = _printerDiscovery.GetInstalledPrinterNames();
        InstalledPrinters.Clear();
        foreach (var p in list)
            InstalledPrinters.Add(p);
        if (InstalledPrinters.Count > 0 && string.IsNullOrEmpty(NewPrinterName))
            NewPrinterName = InstalledPrinters[0];
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

    private void PersistAndClearForm()
    {
        Persist();
        ClearForm();
    }

    private void ClearForm()
    {
        NewName = string.Empty;
        NewWatchPath = string.Empty;
        NewFilePattern = string.Empty;
        NewPrinterName = InstalledPrinters.Count > 0 ? InstalledPrinters[0] : string.Empty;
    }
}
