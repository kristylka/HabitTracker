namespace HabitTracker.App.ViewModels;

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HabitTracker.Core.Messages;
using HabitTracker.Core.Models;

public partial class ProfileViewModel : ViewModelBase
{
    private readonly IMessenger _messenger;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private DateTime _memberSince;

    public ProfileViewModel(IMessenger messenger)
    {
        _messenger = messenger;
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
}