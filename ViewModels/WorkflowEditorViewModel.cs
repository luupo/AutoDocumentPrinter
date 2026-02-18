using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using PrintMaster.Models;
using PrintMaster.Services;

namespace PrintMaster.ViewModels;

public class WorkflowEditorViewModel : ViewModelBase
{
    private readonly PrintWorkflow? _original;
    private string _name = string.Empty;
    private string _watchPath = string.Empty;
    private string _filePattern = "*.*";
    private string _printerName = string.Empty;
    private string _errorMessage = string.Empty;
    private int _delaySeconds = 1;
    private PostActionType _postAction = PostActionType.None;
    private string _moveToPath = string.Empty;
    private string _renameTo = string.Empty;
    private bool _useRegexPattern;
    private bool _isEnabled = true;

    public WorkflowEditorViewModel(
        string title,
        IReadOnlyList<string> installedPrinters,
        PrintWorkflow? workflowToEdit = null)
    {
        Title = title;
        _original = workflowToEdit;
        if (workflowToEdit != null)
        {
            _name = workflowToEdit.Name;
            _watchPath = workflowToEdit.WatchPath;
            _filePattern = string.IsNullOrWhiteSpace(workflowToEdit.FilePattern) ? "*.*" : workflowToEdit.FilePattern;
            _printerName = workflowToEdit.PrinterName;
            _delaySeconds = workflowToEdit.DelaySeconds;
            if (_delaySeconds < 0) _delaySeconds = 1;
            _postAction = workflowToEdit.PostAction;
            _moveToPath = workflowToEdit.MoveToPath ?? string.Empty;
            _renameTo = workflowToEdit.RenameTo ?? string.Empty;
            _useRegexPattern = workflowToEdit.UseRegexPattern;
            _isEnabled = workflowToEdit.IsEnabled;
        }

        InstalledPrinters = new ObservableCollection<string>(installedPrinters);
        if (InstalledPrinters.Count > 0 && string.IsNullOrEmpty(_printerName))
            _printerName = InstalledPrinters[0];

        PickFolderCommand = new RelayCommand(PickFolder);
        PickMoveFolderCommand = new RelayCommand(PickMoveFolder);
        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
    }

    public string Title { get; }
    public ObservableCollection<string> InstalledPrinters { get; }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value ?? string.Empty);
    }

    public string WatchPath
    {
        get => _watchPath;
        set => SetProperty(ref _watchPath, value ?? string.Empty);
    }

    public string FilePattern
    {
        get => _filePattern;
        set => SetProperty(ref _filePattern, value ?? string.Empty);
    }

    public string PrinterName
    {
        get => _printerName;
        set => SetProperty(ref _printerName, value ?? string.Empty);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value ?? string.Empty);
    }

    /// <summary>Wartezeit in Sekunden nach Erkennung der Datei (min. 0, max. 300).</summary>
    public int DelaySeconds
    {
        get => _delaySeconds;
        set => SetProperty(ref _delaySeconds, Math.Clamp(value, 0, 300));
    }

    public PostActionType PostAction
    {
        get => _postAction;
        set => SetProperty(ref _postAction, value);
    }

    public string MoveToPath
    {
        get => _moveToPath;
        set => SetProperty(ref _moveToPath, value ?? string.Empty);
    }

    public string RenameTo
    {
        get => _renameTo;
        set => SetProperty(ref _renameTo, value ?? string.Empty);
    }

    public bool UseRegexPattern
    {
        get => _useRegexPattern;
        set => SetProperty(ref _useRegexPattern, value);
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }

    public Array PostActionTypes { get; } = Enum.GetValues(typeof(PostActionType));

    public PrintWorkflow? ResultWorkflow { get; private set; }
    public bool DialogResult { get; private set; }

    public ICommand PickFolderCommand { get; }
    public ICommand PickMoveFolderCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    private void PickFolder()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = LocalizationService.T("Loc_We_Dialog_PickWatch"),
            UseDescriptionForTitle = true
        };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            WatchPath = dialog.SelectedPath;
    }

    private void PickMoveFolder()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = LocalizationService.T("Loc_We_Dialog_PickMove"),
            UseDescriptionForTitle = true
        };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            MoveToPath = dialog.SelectedPath;
    }

    private void Save()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = LocalizationService.T("Loc_We_Err_Name");
            return;
        }
        if (string.IsNullOrWhiteSpace(WatchPath) || !Directory.Exists(WatchPath))
        {
            ErrorMessage = LocalizationService.T("Loc_We_Err_Folder");
            return;
        }
        if (string.IsNullOrWhiteSpace(PrinterName))
        {
            ErrorMessage = LocalizationService.T("Loc_We_Err_Printer");
            return;
        }
        if (PostAction == PostActionType.Move && (string.IsNullOrWhiteSpace(MoveToPath) || !Directory.Exists(MoveToPath.Trim())))
        {
            ErrorMessage = LocalizationService.T("Loc_We_Err_MoveFolder");
            return;
        }
        if (PostAction == PostActionType.Rename && string.IsNullOrWhiteSpace(RenameTo))
        {
            ErrorMessage = LocalizationService.T("Loc_We_Err_Rename");
            return;
        }

        ResultWorkflow = new PrintWorkflow
        {
            Id = _original?.Id ?? Guid.NewGuid(),
            Name = Name.Trim(),
            WatchPath = WatchPath.Trim(),
            FilePattern = string.IsNullOrWhiteSpace(FilePattern) ? "*.*" : FilePattern.Trim(),
            PrinterName = PrinterName,
            DelaySeconds = DelaySeconds,
            PostAction = PostAction,
            MoveToPath = MoveToPath.Trim(),
            RenameTo = RenameTo.Trim(),
            UseRegexPattern = UseRegexPattern,
            IsEnabled = IsEnabled
        };
        DialogResult = true;
        CloseRequested?.Invoke();
    }

    private void Cancel()
    {
        DialogResult = false;
        CloseRequested?.Invoke();
    }

    /// <summary>Wird ausgelöst, wenn das Fenster geschlossen werden soll (Save oder Cancel).</summary>
    public event Action? CloseRequested;
}
