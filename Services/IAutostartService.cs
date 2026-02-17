namespace PrintMaster.Services;

public interface IAutostartService
{
    bool IsAutostartEnabled { get; }
    void SetAutostart(bool enable);
}
