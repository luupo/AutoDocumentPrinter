using System.Collections.Concurrent;
using System.IO;
using PrintMaster.Models;

namespace PrintMaster.Services;

public class FileWatcherService : IFileWatcherService
{
    private readonly IPrintService _printService;
    private readonly ILogService _logService;
    private readonly ConcurrentDictionary<Guid, FileSystemWatcher> _watchers = new();
    /// <summary>Dateien, die beim Start der Überwachung bereits im Ordner lagen – werden ignoriert.</summary>
    private readonly ConcurrentDictionary<Guid, HashSet<string>> _initialFilePaths = new();

    public FileWatcherService(IPrintService printService, ILogService logService)
    {
        _printService = printService;
        _logService = logService;
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
            var watchPath = Path.GetFullPath(workflow.WatchPath);
            var initialPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var path in Directory.EnumerateFiles(watchPath, "*", SearchOption.TopDirectoryOnly))
                {
                    var name = Path.GetFileName(path);
                    if (MatchesPattern(name, workflow.FilePattern, workflow.UseRegexPattern))
                        initialPaths.Add(Path.GetFullPath(path));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FileWatcherService: Snapshot vorhandener Dateien: {ex.Message}");
            }

            _initialFilePaths[workflow.Id] = initialPaths;

            var watcher = new FileSystemWatcher(watchPath)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite,
                Filter = "*.*"
            };

            watcher.Created += (_, e) => OnFileDetected(e.Name ?? "", e.FullPath ?? "", workflow);
            watcher.Renamed += (_, e) => OnFileDetected(e.Name ?? "", e.FullPath ?? "", workflow);
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
        _initialFilePaths.TryRemove(workflowId, out _);
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

    private async void OnFileDetected(string fileName, string fullPath, PrintWorkflow workflow)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return;
        if (!MatchesPattern(fileName, workflow.FilePattern, workflow.UseRegexPattern))
            return;

        var normalizedPath = Path.GetFullPath(fullPath);
        if (_initialFilePaths.TryGetValue(workflow.Id, out var initial) && initial.Contains(normalizedPath))
            return;

        var delayMs = Math.Max(0, workflow.DelaySeconds) * 1000;
        if (delayMs < 100) delayMs = 100;
        await Task.Delay(delayMs).ConfigureAwait(false);

        if (!File.Exists(fullPath))
            return;

        try
        {
            var success = await _printService.PrintAsync(fullPath, workflow.PrinterName).ConfigureAwait(false);
            if (success)
            {
                _logService.LogSuccess(fullPath, workflow.Name);
                if (workflow.PostAction != PostActionType.None)
                    ExecutePostAction(fullPath, workflow);
            }
            else
                _logService.LogFailure(fullPath, workflow.Name, "Druckauftrag fehlgeschlagen.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"FileWatcherService.OnFileDetected: {ex.Message}");
            _logService.LogFailure(fullPath, workflow.Name, ex.Message);
        }
    }

    private static void ExecutePostAction(string filePath, PrintWorkflow workflow)
    {
        if (!File.Exists(filePath)) return;
        try
        {
            switch (workflow.PostAction)
            {
                case PostActionType.Delete:
                    File.Delete(filePath);
                    System.Diagnostics.Debug.WriteLine($"PostAction: Gelöscht {filePath}");
                    break;
                case PostActionType.Move:
                    if (string.IsNullOrWhiteSpace(workflow.MoveToPath) || !Directory.Exists(workflow.MoveToPath))
                        break;
                    var destPath = Path.Combine(workflow.MoveToPath.Trim(), Path.GetFileName(filePath));
                    if (destPath != filePath)
                    {
                        if (File.Exists(destPath)) File.Delete(destPath);
                        File.Move(filePath, destPath);
                        System.Diagnostics.Debug.WriteLine($"PostAction: Verschoben nach {destPath}");
                    }
                    break;
                case PostActionType.Rename:
                    if (string.IsNullOrWhiteSpace(workflow.RenameTo)) break;
                    var dir = Path.GetDirectoryName(filePath);
                    if (string.IsNullOrEmpty(dir)) break;
                    var newPath = Path.Combine(dir, workflow.RenameTo.Trim());
                    if (newPath != filePath)
                    {
                        if (File.Exists(newPath)) File.Delete(newPath);
                        File.Move(filePath, newPath);
                        System.Diagnostics.Debug.WriteLine($"PostAction: Umbenannt nach {newPath}");
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"FileWatcherService.ExecutePostAction: {ex.Message}");
        }
    }

    private static bool MatchesPattern(string fileName, string pattern, bool useRegex)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return true;

        if (useRegex)
        {
            try
            {
                return System.Text.RegularExpressions.Regex.IsMatch(fileName, pattern);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";

        return System.Text.RegularExpressions.Regex.IsMatch(fileName, regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}
