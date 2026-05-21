namespace HabitTracker.Desktop.ViewModels;

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HabitTracker.Core.Interfaces;
using HabitTracker.Core.Messages;
using HabitTracker.Core.Models;

public partial class ProfileViewModel : ViewModelBase
{
    private readonly IMessenger _messenger;
    private readonly INotificationService _notificationService;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private DateTime _memberSince;

    public ProfileViewModel(IMessenger messenger, INotificationService notificationService)
    {
        _messenger = messenger;
        _notificationService = notificationService;
    }

    public void SetUser(User user)
    {
        DisplayName = user.DisplayName;
        Username = user.Username;
        MemberSince = user.CreatedAt;
    }

    [RelayCommand]
    private void Logout()
    {
        _messenger.Send(new UserLoggedOutMessage());
    }

    [RelayCommand]
    private void TestNotification()
    {
        _notificationService.ShowNotification(
            "Время привычки! 🌱",
            "Не забудь выпить стакан воды");
    }
}