namespace HabitTracker.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HabitTracker.Core.Messages;
using HabitTracker.Core.Models;

public partial class MainViewModel : ViewModelBase,
    IRecipient<UserLoggedInMessage>,
    IRecipient<UserLoggedOutMessage>,
    IRecipient<NavigationMessage>
{
    private readonly IMessenger _messenger;
    private readonly LoginViewModel _loginVm;
    private readonly RegisterViewModel _registerVm;
    private readonly CalendarViewModel _calendarVm;
    private readonly AnalyticsViewModel _analyticsVm;
    private readonly ProfileViewModel _profileVm;

    private User? _currentUser;

    [ObservableProperty]
    private ViewModelBase _currentView;

    [ObservableProperty]
    private bool _isLoggedIn;

    [ObservableProperty]
    private string _currentViewName = "Calendar";

    public MainViewModel(
        IMessenger messenger,
        LoginViewModel loginVm,
        RegisterViewModel registerVm,
        CalendarViewModel calendarVm,
        AnalyticsViewModel analyticsVm,
        ProfileViewModel profileVm)
    {
        _messenger = messenger;
        _loginVm = loginVm;
        _registerVm = registerVm;
        _calendarVm = calendarVm;
        _analyticsVm = analyticsVm;
        _profileVm = profileVm;

        _currentView = _loginVm;

        _messenger.Register<UserLoggedInMessage>(this);
        _messenger.Register<UserLoggedOutMessage>(this);
        _messenger.Register<NavigationMessage>(this);
    }

    public void Receive(UserLoggedInMessage message)
    {
        _currentUser = message.User;
        IsLoggedIn = true;

        _calendarVm.SetUser(_currentUser.Id);
        _analyticsVm.SetUser(_currentUser.Id);
        _profileVm.SetUser(_currentUser);

        NavigateToCalendar();
    }

    public void Receive(UserLoggedOutMessage message)
    {
        _currentUser = null;
        IsLoggedIn = false;
        CurrentView = _loginVm;
        CurrentViewName = "Login";
    }

    public void Receive(NavigationMessage message)
    {
        switch (message.ViewName)
        {
            case "Login":
                CurrentView = _loginVm;
                break;
            case "Register":
                CurrentView = _registerVm;
                break;
            case "Calendar":
                NavigateToCalendar();
                break;
            case "Analytics":
                NavigateToAnalytics();
                break;
            case "Profile":
                NavigateToProfile();
                break;
        }
    }

    [RelayCommand]
    private void NavigateToCalendar()
    {
        CurrentViewName = "Calendar";
        CurrentView = _calendarVm;
        _calendarVm.LoadEntriesCommand.Execute(null);
    }

    [RelayCommand]
    private void NavigateToAnalytics()
    {
        CurrentViewName = "Analytics";
        CurrentView = _analyticsVm;
        _analyticsVm.LoadAnalyticsCommand.Execute(null);
    }

    [RelayCommand]
    private void NavigateToProfile()
    {
        CurrentViewName = "Profile";
        CurrentView = _profileVm;
    }

    [RelayCommand]
    private void Logout()
    {
        _messenger.Send(new UserLoggedOutMessage());
    }
}