using Avalonia.Threading;
using HabitTracker.Core.Interfaces;
using HabitTracker.Desktop.Views;

namespace HabitTracker.Desktop.Services;

public class NotificationService : INotificationService
{
    public void ShowNotification(string title, string message)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var toast = new ToastWindow();
            toast.ShowToast(title, message);
        });
    }
}