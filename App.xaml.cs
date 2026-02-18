using PrintMaster.Services;
using PrintMaster.ViewModels;
using System.Runtime.InteropServices;
using System.Threading;
using System;
using System.Windows;
using System.Diagnostics;

namespace PrintMaster;

public partial class App : System.Windows.Application
{
    private static Mutex? _singleInstanceMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        const string mutexName = "PrintMaster_SingleInstance_Mutex";
        _singleInstanceMutex = new Mutex(true, mutexName, out var createdNew);
        if (!createdNew)
        {
            BringExistingInstanceToFront();
            Shutdown();
            return;
        }

        base.OnStartup(e);
        LocalizationService.ApplyLanguage();
        ThemeService.ApplyTheme();
        var mainWindow = new MainWindow
        {
            DataContext = ViewModelLocator.Main
        };
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();
        _singleInstanceMutex = null;
        base.OnExit(e);
    }

    private static void BringExistingInstanceToFront()
    {
        var current = Process.GetCurrentProcess();
        var name = current.ProcessName;
        const int SW_RESTORE = 9;
        foreach (var p in Process.GetProcessesByName(name))
        {
            if (p.Id == current.Id) continue;
            try
            {
                p.Refresh();
                var hWnd = p.MainWindowHandle;
                if (hWnd == IntPtr.Zero) continue;
                ShowWindowAsync(hWnd, SW_RESTORE);
                SetForegroundWindow(hWnd);
                return;
            }
            catch { /* ignore */ }
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
}
