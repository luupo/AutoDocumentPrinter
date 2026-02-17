using System.Windows;
using PrintMaster.ViewModels;
using PrintMaster.Services;

namespace PrintMaster;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel(new AutostartService());
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            vm.OkCommand.Execute(null);
        DialogResult = true;
        Close();
    }
}
