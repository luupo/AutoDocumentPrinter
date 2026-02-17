using System.Windows;
using PrintMaster.ViewModels;

namespace PrintMaster;

public partial class LogWindow : Window
{
    public LogWindow()
    {
        InitializeComponent();
        DataContext = ViewModelLocator.LogService;
    }
}
