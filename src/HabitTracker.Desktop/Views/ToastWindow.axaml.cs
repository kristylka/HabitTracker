using System;
using Avalonia.Controls;
using Avalonia.Threading;

namespace HabitTracker.Desktop.Views;

public partial class ToastWindow : Window
{
    private readonly DispatcherTimer _timer;

    public ToastWindow()
    {
        InitializeComponent();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _timer.Tick += (s, e) =>
        {
            _timer.Stop();
            Close();
        };
    }

    public void ShowToast(string title, string message)
    {
        TitleText.Text = title;
        MessageText.Text = message;

        var screen = Screens.Primary;
        if (screen != null)
        {
            var bounds = screen.WorkingArea;
            Position = new Avalonia.PixelPoint(
                bounds.X + bounds.Width - 360,
                bounds.Y + 40);
        }

        Show();
        _timer.Start();
    }
}