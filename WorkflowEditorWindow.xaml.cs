using System.IO;
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

    private void PipelineDropTarget_DragOver(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop) && e.Data.GetData(System.Windows.DataFormats.FileDrop) is string[] files
            && files.Length > 0 && File.Exists(files[0]) && !Directory.Exists(files[0]))
            e.Effects = System.Windows.DragDropEffects.Copy;
        else
            e.Effects = System.Windows.DragDropEffects.None;
        e.Handled = true;
    }

    private void PipelineDropTarget_Drop(object sender, System.Windows.DragEventArgs e)
    {
        if (DataContext is not WorkflowEditorViewModel vm) return;
        if (!e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop) || e.Data.GetData(System.Windows.DataFormats.FileDrop) is not string[] files
            || files.Length == 0) return;

        var path = files[0];
        if (string.IsNullOrEmpty(path) || !File.Exists(path) || Directory.Exists(path)) return;

        var dir = Path.GetDirectoryName(path);
        var fileName = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(dir))
            vm.WatchPath = dir;
        if (!string.IsNullOrEmpty(fileName))
            vm.FilePattern = fileName;

        e.Handled = true;
    }
}
