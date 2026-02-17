using System.Windows;
using PrintMaster.ViewModels;

namespace PrintMaster;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var mainWindow = new MainWindow
        {
            DataContext = ViewModelLocator.Main
        };
        mainWindow.Show();
    }
}
