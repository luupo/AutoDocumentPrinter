using System.IO;
using System.IO.Compression;

namespace PrintMaster.Services;

public class BackupService : IBackupService
{
    private readonly string _appFolder;
    public string BackupFolder { get; }

    public BackupService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _appFolder = Path.Combine(appData, "PrintMaster");
        Directory.CreateDirectory(_appFolder);
        BackupFolder = Path.Combine(_appFolder, "Backups");
        Directory.CreateDirectory(BackupFolder);
    }

    public string CreateBackup()
    {
        var fileName = $"AutoDocPrinter_backup_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
        var targetPath = Path.Combine(BackupFolder, fileName);

        using var zip = ZipFile.Open(targetPath, ZipArchiveMode.Create);

        // Alle relevanten JSON-Konfigurationsdateien sichern
        foreach (var file in Directory.EnumerateFiles(_appFolder, "*.json", SearchOption.TopDirectoryOnly))
        {
            var name = Path.GetFileName(file);
            if (name is null) continue;
            zip.CreateEntryFromFile(file, name, CompressionLevel.Optimal);
        }

        return targetPath;
    }

    public void RestoreBackup(string backupFilePath)
    {
        if (!File.Exists(backupFilePath))
            throw new FileNotFoundException("Backup-Datei wurde nicht gefunden.", backupFilePath);

        using var zip = ZipFile.OpenRead(backupFilePath);
        foreach (var entry in zip.Entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Name)) continue;
            var targetName = entry.Name;
            // Migration: beliebige *_workflows.json oder ähnliche Namen als workflows.json behandeln
            if (entry.Name.Equals("workflows.json", StringComparison.OrdinalIgnoreCase) ||
                entry.Name.EndsWith("_workflows.json", StringComparison.OrdinalIgnoreCase))
            {
                targetName = "workflows.json";
            }

            var destinationPath = Path.Combine(_appFolder, targetName);
            entry.ExtractToFile(destinationPath, overwrite: true);
        }
    }
}

