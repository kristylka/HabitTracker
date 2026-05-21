using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HabitTracker.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace HabitTracker.Desktop;

public partial class App : Application
{
    public static IServiceProvider? Services { get; set; }
    private Window? _mainWindow;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _mainWindow = Services!.GetRequiredService<MainWindow>();
            desktop.MainWindow = _mainWindow;

            _mainWindow.Closing += MainWindow_OnClosing;

            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void MainWindow_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        e.Cancel = true;
        _mainWindow?.Hide();
    }

    private void TrayIcon_OnClicked(object? sender, EventArgs e)
    {
        ShowMainWindow();
    }

    private void OpenMenuItem_OnClick(object? sender, EventArgs e)
    {
        ShowMainWindow();
    }

    private void ExitMenuItem_OnClick(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (_mainWindow != null)
                _mainWindow.Closing -= MainWindow_OnClosing;

            desktop.Shutdown();
        }
    }

    private void ShowMainWindow()
    {
        if (_mainWindow == null) return;

        if (!_mainWindow.IsVisible)
            _mainWindow.Show();

        if (_mainWindow.WindowState == WindowState.Minimized)
            _mainWindow.WindowState = WindowState.Normal;

        _mainWindow.Activate();
    }
}