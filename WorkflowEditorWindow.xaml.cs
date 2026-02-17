using System.IO;
using System.Text.Json;
using System.Windows;
using PrintMaster.ViewModels;

namespace PrintMaster;

public partial class WorkflowEditorWindow : Window
{
    private static string PlacementPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PrintMaster", "workfloweditor.json");

    public WorkflowEditorWindow(WorkflowEditorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseRequested += OnCloseRequested;
        Loaded += (_, _) => RestorePlacement();
        Closing += (_, _) => SavePlacement();
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

    private void RestorePlacement()
    {
        try
        {
            var path = PlacementPath;
            if (!File.Exists(path)) return;
            var json = File.ReadAllText(path);
            var doc = JsonDocument.Parse(json);
            var r = doc.RootElement;
            if (r.TryGetProperty("left", out var l) && r.TryGetProperty("top", out var t) &&
                r.TryGetProperty("width", out var w) && r.TryGetProperty("height", out var h))
            {
                var left = l.GetDouble();
                var top = t.GetDouble();
                var width = w.GetDouble();
                var height = h.GetDouble();
                if (width >= MinWidth && width <= 4096 && height >= MinHeight && height <= 4096)
                {
                    Left = left;
                    Top = top;
                    Width = width;
                    Height = height;
                }
                if (r.TryGetProperty("state", out var s) && s.TryGetInt32(out var state) && state == 2)
                    WindowState = WindowState.Maximized;
            }
        }
        catch { /* ignore */ }
    }

    private void SavePlacement()
    {
        try
        {
            var dir = Path.GetDirectoryName(PlacementPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            var state = WindowState == WindowState.Maximized ? 2 : (WindowState == WindowState.Minimized ? 1 : 0);
            var left = Left;
            var top = Top;
            var width = Width;
            var height = Height;
            if (WindowState == WindowState.Maximized && RestoreBounds != default)
            {
                left = RestoreBounds.Left;
                top = RestoreBounds.Top;
                width = RestoreBounds.Width;
                height = RestoreBounds.Height;
            }
            var obj = new { left, top, width, height, state };
            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(PlacementPath, json);
        }
        catch { /* ignore */ }
    }
}
