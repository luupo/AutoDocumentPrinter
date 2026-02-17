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

    /// <summary>Wartezeit in Sekunden nach Erkennung der Datei, bevor gedruckt wird (z. B. bis Schreibvorgang abgeschlossen ist).</summary>
    public int DelaySeconds { get; set; } = 1;

    /// <summary>Optionale Aktion nach der Verarbeitung (z. B. Datei löschen, verschieben, umbenennen).</summary>
    public PostActionType PostAction { get; set; } = PostActionType.None;

    /// <summary>Zielordner bei PostAction.Move.</summary>
    public string MoveToPath { get; set; } = string.Empty;

    /// <summary>Neuer Dateiname bei PostAction.Rename (im selben Ordner).</summary>
    public string RenameTo { get; set; } = string.Empty;
}
