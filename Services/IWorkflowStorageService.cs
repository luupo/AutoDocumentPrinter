using PrintMaster.Models;

namespace PrintMaster.Services;

public interface IWorkflowStorageService
{
    IReadOnlyList<PrintWorkflow> Load();
    void Save(IEnumerable<PrintWorkflow> workflows);
}
