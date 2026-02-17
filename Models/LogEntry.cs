namespace PrintMaster.Models;

public class LogEntry
{
    public DateTime Time { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string WorkflowName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
