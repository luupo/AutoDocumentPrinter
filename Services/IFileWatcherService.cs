using PrintMaster.Models;

namespace PrintMaster.Services;

public interface IFileWatcherService
{
    void StartWatching(PrintWorkflow workflow);
    void StopWatching(Guid workflowId);
    void StopAll();
    bool IsWatching(Guid workflowId);
}
