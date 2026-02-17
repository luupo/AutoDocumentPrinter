using System.Linq;
using System.Windows;
using System.Windows.Input;
using PrintMaster.ViewModels;

namespace PrintMaster;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private MainViewModel? MainVm => DataContext as MainViewModel;

    private void BtnNewWorkflow_Click(object sender, RoutedEventArgs e)
    {
        var vm = MainVm;
        if (vm == null) return;
        vm.RefreshPrintersCommand.Execute(null);
        var editorVm = new WorkflowEditorViewModel("Neuer Workflow", vm.InstalledPrinters.ToList(), null);
        var win = new WorkflowEditorWindow(editorVm) { Owner = this };
        if (win.ShowDialog() == true && editorVm.ResultWorkflow != null)
            vm.AddWorkflowFromEditor(editorVm.ResultWorkflow);
    }

    private void BtnEditWorkflow_Click(object sender, RoutedEventArgs e)
    {
        var vm = MainVm;
        if (vm == null) return;
        if (vm.SelectedWorkflow == null)
        {
            vm.StatusMessage = "Bitte einen Workflow auswählen.";
            return;
        }
        vm.RefreshPrintersCommand.Execute(null);
        var editorVm = new WorkflowEditorViewModel("Workflow bearbeiten", vm.InstalledPrinters.ToList(), vm.SelectedWorkflow);
        var win = new WorkflowEditorWindow(editorVm) { Owner = this };
        if (win.ShowDialog() == true && editorVm.ResultWorkflow != null)
            vm.UpdateWorkflowFromEditor(editorVm.ResultWorkflow);
    }

    private void WorkflowsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (WorkflowsGrid.SelectedItem != null)
            BtnEditWorkflow_Click(sender, e);
    }
}
