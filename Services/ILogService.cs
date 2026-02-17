using System.Collections.ObjectModel;
using PrintMaster.Models;

namespace PrintMaster.Services;

public interface ILogService
{
    ObservableCollection<LogEntry> Entries { get; }
    void LogSuccess(string filePath, string workflowName);
    void LogFailure(string filePath, string workflowName, string message);
    /// <summary>Wird ausgelöst bei Druckfehler (für Tray-Benachrichtigung).</summary>
    event Action<string, string>? PrintFailed;
}
