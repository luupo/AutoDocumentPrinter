using System.Collections.Generic;
using System.Windows.Input;
using PrintMaster.Services;
using Microsoft.Win32;

namespace PrintMaster.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly IAutostartService _autostartService;
    private readonly IBackupService _backupService;
    private bool _autostartEnabled;
    private string _selectedLanguage = LocalizationService.SystemLanguage;

    public SettingsViewModel(IAutostartService autostartService, IBackupService backupService)
    {
        _autostartService = autostartService;
        _backupService = backupService;
        _autostartEnabled = autostartService.IsAutostartEnabled;
        _selectedLanguage = LocalizationService.GetSavedLanguage();
        OkCommand = new RelayCommand(Ok);
        CreateBackupCommand = new RelayCommand(CreateBackup);
        RestoreBackupCommand = new RelayCommand(RestoreBackup);
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
    public ICommand CreateBackupCommand { get; }
    public ICommand RestoreBackupCommand { get; }

    private void Ok()
    {
        _autostartService.SetAutostart(AutostartEnabled);

        LocalizationService.SaveLanguage(SelectedLanguage);
        LocalizationService.ApplyLanguage();
    }

    private void CreateBackup()
    {
        try
        {
            var path = _backupService.CreateBackup();
            System.Windows.MessageBox.Show(
                LocalizationService.F("Loc_Backup_Created", path),
                LocalizationService.T("Loc_Settings_Title"),
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                LocalizationService.F("Loc_Backup_CreateFailed", ex.Message),
                LocalizationService.T("Loc_Settings_Title"),
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    private void RestoreBackup()
    {
        try
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "AutoDocPrinter Backup (*.zip)|*.zip|Alle Dateien (*.*)|*.*",
                InitialDirectory = _backupService.BackupFolder,
                Title = LocalizationService.T("Loc_Backup_OpenDialogTitle")
            };
            var result = dlg.ShowDialog();
            if (result != true) return;

            _backupService.RestoreBackup(dlg.FileName);

            System.Windows.MessageBox.Show(
                LocalizationService.T("Loc_Backup_Restored"),
                LocalizationService.T("Loc_Settings_Title"),
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                LocalizationService.F("Loc_Backup_RestoreFailed", ex.Message),
                LocalizationService.T("Loc_Settings_Title"),
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }
}

public record LanguageChoice(string Display, string Value);
