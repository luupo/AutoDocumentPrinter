namespace PrintMaster.Services;

public interface IBackupService
{
    /// <summary>Erstellt ein Backup (z. B. ZIP) der aktuellen Konfiguration und gibt den Pfad zur Datei zurück.</summary>
    string CreateBackup();

    /// <summary>Stellt ein zuvor erstelltes Backup aus der angegebenen Datei wieder her.</summary>
    void RestoreBackup(string backupFilePath);

    /// <summary>Standardordner, in dem Backups abgelegt werden.</summary>
    string BackupFolder { get; }
}

