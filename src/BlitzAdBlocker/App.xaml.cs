using System;
using System.Threading;
using System.Windows;

namespace BlitzAdBlocker;

public partial class App : Application
{
    private Mutex _singleInstance;

    protected override void OnStartup(StartupEventArgs e)
    {
        _singleInstance = new Mutex(true, @"Local\BlitzAdBlocker.SingleInstance", out var isNew);
        if (!isNew)
        {
            MessageBox.Show("Blitz AdBlocker is already running.", "Blitz AdBlocker",
                MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }
        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _singleInstance?.ReleaseMutex();
        _singleInstance?.Dispose();
        base.OnExit(e);
    }
}