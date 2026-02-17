using System.Collections.Concurrent;
using System.IO;
using PrintMaster.Models;

namespace PrintMaster.Services;

public class FileWatcherService : IFileWatcherService
{
    private readonly IPrintService _printService;
    private readonly ConcurrentDictionary<Guid, FileSystemWatcher> _watchers = new();

    public FileWatcherService(IPrintService printService)
    {
        _printService = printService;
    }

    public void StartWatching(PrintWorkflow workflow)
    {
        if (workflow == null || !workflow.IsEnabled)
            return;
        if (string.IsNullOrWhiteSpace(workflow.WatchPath) || !Directory.Exists(workflow.WatchPath))
            return;

        StopWatching(workflow.Id);

        try
        {
            var watcher = new FileSystemWatcher(workflow.WatchPath)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime,
                Filter = "*.*"
            };

            watcher.Created += (_, e) => OnFileCreated(e, workflow);
            watcher.EnableRaisingEvents = true;

            _watchers[workflow.Id] = watcher;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"FileWatcherService.StartWatching: {ex.Message}");
        }
    }

    public void StopWatching(Guid workflowId)
    {
        if (_watchers.TryRemove(workflowId, out var watcher))
        {
            try
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FileWatcherService.StopWatching: {ex.Message}");
            }
        }
    }

    public void StopAll()
    {
        foreach (var id in _watchers.Keys.ToList())
            StopWatching(id);
    }

    public bool IsWatching(Guid workflowId) => _watchers.ContainsKey(workflowId);

    private async void OnFileCreated(FileSystemEventArgs e, PrintWorkflow workflow)
    {
        if (string.IsNullOrWhiteSpace(e.Name))
            return;
        if (!MatchesPattern(e.Name, workflow.FilePattern))
            return;

        // Kurz warten, falls Datei noch geschrieben wird
        await Task.Delay(500).ConfigureAwait(false);

        var path = e.FullPath;
        if (!File.Exists(path))
            return;

        try
        {
            var success = await _printService.PrintAsync(path, workflow.PrinterName).ConfigureAwait(false);
            System.Diagnostics.Debug.WriteLine(success
                ? $"Gedruckt: {path} -> {workflow.PrinterName}"
                : $"Druck fehlgeschlagen: {path}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"FileWatcherService.OnFileCreated: {ex.Message}");
        }
    }

    private static bool MatchesPattern(string fileName, string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return true;

        // Einfache Wildcard-Unterstützung: * als Platzhalter
        var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";

        return System.Text.RegularExpressions.Regex.IsMatch(fileName, regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}
