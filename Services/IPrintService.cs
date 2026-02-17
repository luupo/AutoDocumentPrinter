namespace PrintMaster.Services;

public interface IPrintService
{
    /// <summary>
    /// Sendet eine Datei an den angegebenen Drucker.
    /// </summary>
    /// <param name="filePath">Vollständiger Pfad zur Datei.</param>
    /// <param name="printerName">Name des Windows-Druckers.</param>
    /// <param name="cancellationToken">Abbruch-Token.</param>
    /// <returns>True bei Erfolg, sonst False.</returns>
    Task<bool> PrintAsync(string filePath, string printerName, CancellationToken cancellationToken = default);
}
