namespace PrintMaster.Models;

/// <summary>
/// Repräsentiert einen Druck-Workflow: Überwachter Ordner, Dateimuster und Ziel-Drucker.
/// </summary>
public class PrintWorkflow
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Anzeigename des Workflows.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Pfad des zu überwachenden Ordners (z.B. C:\Users\Name\Downloads).</summary>
    public string WatchPath { get; set; } = string.Empty;

    /// <summary>Dateimuster (z.B. Invoice*.pdf oder Versand*).</summary>
    public string FilePattern { get; set; } = string.Empty;

    /// <summary>Name des Windows-Druckers.</summary>
    public string PrinterName { get; set; } = string.Empty;

    /// <summary>Workflow aktiv (Überwachung und Druck an).</summary>
    public bool IsEnabled { get; set; } = true;
}
