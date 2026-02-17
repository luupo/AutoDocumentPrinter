using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using PrintMaster.ViewModels;

namespace PrintMaster;

public partial class MainWindow : Window
{
    private NotifyIcon? _trayIcon;
    private bool _isReallyClosing;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private MainViewModel? MainVm => DataContext as MainViewModel;

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _trayIcon = new NotifyIcon
        {
            Text = "PrintMaster – Klicken zum Öffnen",
            Visible = true
        };
        try
        {
            var exePath = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(exePath) && System.IO.File.Exists(exePath))
            {
                var icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
                if (icon != null)
                    _trayIcon.Icon = icon;
                else
                    _trayIcon.Icon = System.Drawing.SystemIcons.Application;
            }
            else
                _trayIcon.Icon = System.Drawing.SystemIcons.Application;
        }
        catch
        {
            _trayIcon.Icon = System.Drawing.SystemIcons.Application;
        }

        var openItem = new ToolStripMenuItem("Fenster öffnen");
        openItem.Click += (_, _) => { Show(); WindowState = WindowState.Normal; Activate(); };
        var settingsItem = new ToolStripMenuItem("Einstellungen");
        settingsItem.Click += (_, _) => BtnSettings_Click(null!, null!);
        var logItem = new ToolStripMenuItem("Log anzeigen");
        logItem.Click += (_, _) => { Show(); Activate(); BtnLog_Click(null!, null!); };
        var exitItem = new ToolStripMenuItem("Beenden");
        exitItem.Click += (_, _) => { _isReallyClosing = true; Close(); };

        _trayIcon.ContextMenuStrip = new ContextMenuStrip();
        _trayIcon.ContextMenuStrip.Items.Add(openItem);
        _trayIcon.ContextMenuStrip.Items.Add(settingsItem);
        _trayIcon.ContextMenuStrip.Items.Add(logItem);
        _trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
        _trayIcon.ContextMenuStrip.Items.Add(exitItem);

        _trayIcon.DoubleClick += (_, _) => { Show(); WindowState = WindowState.Normal; Activate(); };

        ViewModelLocator.LogService.PrintFailed += LogService_PrintFailed;
    }

    private void LogService_PrintFailed(string filePath, string workflowName)
    {
        _trayIcon?.ShowBalloonTip(5000, "PrintMaster – Druckfehler",
            $"Workflow \"{workflowName}\": Druck fehlgeschlagen.\n{System.IO.Path.GetFileName(filePath)}",
            ToolTipIcon.Error);
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!_isReallyClosing)
        {
            e.Cancel = true;
            Hide();
            _trayIcon?.ShowBalloonTip(2000, "PrintMaster", "In Tray minimiert. Doppelklick zum Öffnen.", ToolTipIcon.Info);
        }
        else
        {
            ViewModelLocator.LogService.PrintFailed -= LogService_PrintFailed;
            _trayIcon?.Dispose();
            _trayIcon = null;
        }
    }

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

    private void BtnSettings_Click(object sender, RoutedEventArgs e)
    {
        var win = new SettingsWindow { Owner = this };
        win.ShowDialog();
    }

    private void BtnLog_Click(object sender, RoutedEventArgs e)
    {
        var win = new LogWindow { Owner = this };
        win.Show();
    }

    private void WorkflowsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (WorkflowsGrid.SelectedItem != null)
            BtnEditWorkflow_Click(sender, e);
    }

    private void WorkflowActive_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.FrameworkElement fe || fe.DataContext is not PrintMaster.Models.PrintWorkflow workflow)
            return;
        MainVm?.ToggleWorkflowEnabled(workflow);
    }
}
