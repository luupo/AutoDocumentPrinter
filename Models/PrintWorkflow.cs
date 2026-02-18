using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PrintMaster.Models;

/// <summary>
/// Repräsentiert einen Druck-Workflow: Überwachter Ordner, Dateimuster und Ziel-Drucker.
/// </summary>
public class PrintWorkflow : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _watchPath = string.Empty;
    private string _filePattern = string.Empty;
    private bool _useRegexPattern;
    private string _printerName = string.Empty;
    private bool _isEnabled = true;
    private int _delaySeconds = 1;
    private PostActionType _postAction = PostActionType.None;
    private string _moveToPath = string.Empty;
    private string _renameTo = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Anzeigename des Workflows.</summary>
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value) return;
            _name = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    /// <summary>Pfad des zu überwachenden Ordners (z.B. C:\Users\Name\Downloads).</summary>
    public string WatchPath
    {
        get => _watchPath;
        set
        {
            if (_watchPath == value) return;
            _watchPath = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    /// <summary>Dateimuster (z.B. Invoice*.pdf oder regulärer Ausdruck).</summary>
    public string FilePattern
    {
        get => _filePattern;
        set
        {
            if (_filePattern == value) return;
            _filePattern = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    /// <summary>Wenn true, wird FilePattern als .NET-Regular-Expression ausgewertet (sonst Wildcards * und ?).</summary>
    public bool UseRegexPattern
    {
        get => _useRegexPattern;
        set
        {
            if (_useRegexPattern == value) return;
            _useRegexPattern = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Name des Windows-Druckers.</summary>
    public string PrinterName
    {
        get => _printerName;
        set
        {
            if (_printerName == value) return;
            _printerName = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    /// <summary>Workflow aktiv (Überwachung und Druck an).</summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set { _isEnabled = value; OnPropertyChanged(); }
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    /// <summary>Wartezeit in Sekunden nach Erkennung der Datei, bevor gedruckt wird (z. B. bis Schreibvorgang abgeschlossen ist).</summary>
    public int DelaySeconds
    {
        get => _delaySeconds;
        set
        {
            if (_delaySeconds == value) return;
            _delaySeconds = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Optionale Aktion nach der Verarbeitung (z. B. Datei löschen, verschieben, umbenennen).</summary>
    public PostActionType PostAction
    {
        get => _postAction;
        set
        {
            if (_postAction == value) return;
            _postAction = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Zielordner bei PostAction.Move.</summary>
    public string MoveToPath
    {
        get => _moveToPath;
        set
        {
            if (_moveToPath == value) return;
            _moveToPath = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    /// <summary>Neuer Dateiname bei PostAction.Rename (im selben Ordner).</summary>
    public string RenameTo
    {
        get => _renameTo;
        set
        {
            if (_renameTo == value) return;
            _renameTo = value ?? string.Empty;
            OnPropertyChanged();
        }
    }
}
