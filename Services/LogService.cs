using System.Collections.ObjectModel;
using PrintMaster.Models;

namespace PrintMaster.Services;

public class LogService : ILogService
{
    private const int MaxEntries = 500;

    public ObservableCollection<LogEntry> Entries { get; } = new();

    public event Action<string, string>? PrintFailed;

    public void LogSuccess(string filePath, string workflowName)
    {
        AddEntry(filePath, workflowName, true, "");
    }

    public void LogFailure(string filePath, string workflowName, string message)
    {
        AddEntry(filePath, workflowName, false, message);
        PrintFailed?.Invoke(filePath, workflowName);
    }

    private void AddEntry(string filePath, string workflowName, bool success, string message)
    {
        var entry = new LogEntry
        {
            Time = DateTime.Now,
            FilePath = filePath,
            WorkflowName = workflowName,
            Success = success,
            Message = message
        };
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            Entries.Insert(0, entry);
            while (Entries.Count > MaxEntries)
                Entries.RemoveAt(Entries.Count - 1);
        });
    }
}
