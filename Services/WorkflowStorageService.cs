using System.IO;
using System.Text.Json;
using PrintMaster.Models;

namespace PrintMaster.Services;

public class WorkflowStorageService : IWorkflowStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _configPath;

    public WorkflowStorageService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appData, "PrintMaster");
        Directory.CreateDirectory(appFolder);
        _configPath = Path.Combine(appFolder, "workflows.json");
    }

    public IReadOnlyList<PrintWorkflow> Load()
    {
        try
        {
            if (!File.Exists(_configPath))
                return new List<PrintWorkflow>();

            var json = File.ReadAllText(_configPath);
            var list = JsonSerializer.Deserialize<List<PrintWorkflow>>(json, JsonOptions);
            return list ?? new List<PrintWorkflow>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"WorkflowStorageService.Load: {ex.Message}");
            return new List<PrintWorkflow>();
        }
    }

    public void Save(IEnumerable<PrintWorkflow> workflows)
    {
        try
        {
            var list = workflows.ToList();
            var json = JsonSerializer.Serialize(list, JsonOptions);
            File.WriteAllText(_configPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"WorkflowStorageService.Save: {ex.Message}");
            throw new InvalidOperationException("Workflows konnten nicht gespeichert werden.", ex);
        }
    }
}
