using System.Windows.Input;
using PrintMaster.Services;

namespace PrintMaster.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly IAutostartService _autostartService;
    private bool _autostartEnabled;

    public SettingsViewModel(IAutostartService autostartService)
    {
        _autostartService = autostartService;
        _autostartEnabled = autostartService.IsAutostartEnabled;
        OkCommand = new RelayCommand(Ok);
    }

    public bool AutostartEnabled
    {
        get => _autostartEnabled;
        set => SetProperty(ref _autostartEnabled, value);
    }

    public ICommand OkCommand { get; }

    private void Ok()
    {
        _autostartService.SetAutostart(AutostartEnabled);
    }
}
