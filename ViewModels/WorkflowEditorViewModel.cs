using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using PrintMaster.Models;
using PrintMaster.Services;

namespace PrintMaster.ViewModels;

public class PatternItem : ViewModelBase
{
    private string _text = string.Empty;
    private bool _isRemovable;

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value ?? string.Empty);
    }

    public bool IsRemovable
    {
        get => _isRemovable;
        set => SetProperty(ref _isRemovable, value);
    }
}

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
    private PatternItem? _activePattern;

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

        // Patterns aus FilePattern initialisieren
        Patterns = new ObservableCollection<PatternItem>();
        var parts = (_filePattern ?? string.Empty)
            .Split(new[] { '\r', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .ToList();
        if (parts.Count == 0)
        {
            Patterns.Add(new PatternItem { Text = string.Empty, IsRemovable = false });
        }
        else
        {
            for (int i = 0; i < parts.Count; i++)
            {
                Patterns.Add(new PatternItem { Text = parts[i], IsRemovable = i > 0 });
            }
        }
        _activePattern = Patterns.FirstOrDefault();

        PickFolderCommand = new RelayCommand(PickFolder);
        PickMoveFolderCommand = new RelayCommand(PickMoveFolder);
        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
        SuggestPatternCommand = new RelayCommand(SuggestPattern);
        AddPatternCommand = new RelayCommand(AddPattern);
        RemovePatternCommand = new RelayCommand(() => RemovePattern(_activePattern));
    }

    public string Title { get; }
    public ObservableCollection<string> InstalledPrinters { get; }
    public ObservableCollection<PatternItem> Patterns { get; }

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
    public ICommand SuggestPatternCommand { get; }
    public ICommand AddPatternCommand { get; }
    public ICommand RemovePatternCommand { get; }

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
        if (string.IsNullOrWhiteSpace(WatchPath))
        {
            ErrorMessage = LocalizationService.T("Loc_We_Err_Folder");
            return;
        }
        if (string.IsNullOrWhiteSpace(PrinterName))
        {
            ErrorMessage = LocalizationService.T("Loc_We_Err_Printer");
            return;
        }
        if (PostAction == PostActionType.Move && string.IsNullOrWhiteSpace(MoveToPath))
        {
            ErrorMessage = LocalizationService.T("Loc_We_Err_MoveFolder");
            return;
        }
        if (PostAction == PostActionType.Rename && string.IsNullOrWhiteSpace(RenameTo))
        {
            ErrorMessage = LocalizationService.T("Loc_We_Err_Rename");
            return;
        }

        // Patterns in ein gemeinsames Muster zusammenführen (eine Zeile pro Pattern)
        var patternLines = Patterns
            .Select(p => (p.Text ?? string.Empty).Trim())
            .Where(p => p.Length > 0)
            .ToList();
        var combinedPattern = patternLines.Count == 0 ? "*.*" : string.Join("\n", patternLines);

        ResultWorkflow = new PrintWorkflow
        {
            Id = _original?.Id ?? Guid.NewGuid(),
            Name = Name.Trim(),
            WatchPath = WatchPath.Trim(),
            FilePattern = combinedPattern,
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

    private void SuggestPattern()
    {
        try
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = LocalizationService.T("Loc_PatternAssist_Title"),
                Filter = "Alle Dateien (*.*)|*.*",
                Multiselect = true
            };

            if (Directory.Exists(WatchPath))
                dlg.InitialDirectory = WatchPath;

            var result = dlg.ShowDialog();
            if (result != true) return;

            var names = dlg.FileNames
                .Select(Path.GetFileName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();

            if (names.Count < 1)
                return;

            var pattern = BuildWildcardPattern(names);
            if (string.IsNullOrEmpty(pattern))
            {
                System.Windows.MessageBox.Show(
                    LocalizationService.T("Loc_PatternAssist_NoResult"),
                    LocalizationService.T("Loc_PatternAssist_Title"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
                return;
            }

            FilePattern = pattern;
            UseRegexPattern = false;

            System.Windows.MessageBox.Show(
                LocalizationService.F("Loc_PatternAssist_Result", pattern),
                LocalizationService.T("Loc_PatternAssist_Title"),
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                ex.Message,
                LocalizationService.T("Loc_PatternAssist_Title"),
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    private static string? BuildWildcardPattern(IReadOnlyList<string> fileNames)
    {
        if (fileNames == null || fileNames.Count == 0)
            return null;

        if (fileNames.Count == 1)
            return fileNames[0];

        string first = fileNames[0];

        // Gemeinsamer Präfix
        var prefix = first;
        foreach (var name in fileNames.Skip(1))
        {
            while (!name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && prefix.Length > 0)
                prefix = prefix[..^1];
            if (prefix.Length == 0) break;
        }

        // Gemeinsamer Suffix
        var suffix = first;
        foreach (var name in fileNames.Skip(1))
        {
            while (!name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) && suffix.Length > 0)
                suffix = suffix[1..];
            if (suffix.Length == 0) break;
        }

        if (string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(suffix))
            return null;

        // Überlappung prüfen
        if (!string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(suffix))
        {
            if (prefix.Length + suffix.Length >= first.Length)
                return first;
            return prefix + "*" + suffix;
        }

        if (!string.IsNullOrEmpty(prefix))
            return prefix + "*";

        return "*" + suffix;
    }

    public void SetActivePattern(PatternItem item)
    {
        _activePattern = item;
    }

    private void AddPattern()
    {
        var newItem = new PatternItem { Text = string.Empty, IsRemovable = true };
        foreach (var p in Patterns)
            p.IsRemovable = Patterns.Count > 0; // erste Box bleibt ohne X
        Patterns.Add(newItem);
        _activePattern = newItem;
    }

    private void RemovePattern(PatternItem? item)
    {
        if (item == null) return;
        if (Patterns.Count <= 1) return;
        Patterns.Remove(item);
        if (!Patterns.Any())
        {
            Patterns.Add(new PatternItem { Text = string.Empty, IsRemovable = false });
        }
        else
        {
            // Erste Box nie entfernbar
            for (int i = 0; i < Patterns.Count; i++)
                Patterns[i].IsRemovable = i > 0;
        }
        _activePattern ??= Patterns.FirstOrDefault();
    }
}
