using System.Windows;
using PrintMaster.ViewModels;

namespace PrintMaster;

public partial class WorkflowEditorWindow : Window
{
    public WorkflowEditorWindow(WorkflowEditorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseRequested += OnCloseRequested;
    }

    private void OnCloseRequested()
    {
        if (DataContext is WorkflowEditorViewModel vm)
        {
            vm.CloseRequested -= OnCloseRequested;
            DialogResult = vm.DialogResult;
        }
        Close();
    }
}
