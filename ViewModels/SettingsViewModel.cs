using System.Collections.Generic;
using System.Windows.Input;
using PrintMaster.Services;

namespace PrintMaster.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly IAutostartService _autostartService;
    private bool _autostartEnabled;
    private string _selectedLanguage = LocalizationService.SystemLanguage;

    public SettingsViewModel(IAutostartService autostartService)
    {
        _autostartService = autostartService;
        _autostartEnabled = autostartService.IsAutostartEnabled;
        _selectedLanguage = LocalizationService.GetSavedLanguage();
        OkCommand = new RelayCommand(Ok);
    }

    public bool AutostartEnabled
    {
        get => _autostartEnabled;
        set => SetProperty(ref _autostartEnabled, value);
    }

    public IReadOnlyList<LanguageChoice> LanguageChoices { get; } = new[]
    {
        new LanguageChoice(LocalizationService.T("Loc_Lang_System"), LocalizationService.SystemLanguage),
        new LanguageChoice(LocalizationService.T("Loc_Lang_English"), LocalizationService.English),
        new LanguageChoice(LocalizationService.T("Loc_Lang_German"), LocalizationService.German)
    };

    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set => SetProperty(ref _selectedLanguage, value);
    }

    public ICommand OkCommand { get; }

    private void Ok()
    {
        _autostartService.SetAutostart(AutostartEnabled);

        LocalizationService.SaveLanguage(SelectedLanguage);
        LocalizationService.ApplyLanguage();
    }
}

public record LanguageChoice(string Display, string Value);
