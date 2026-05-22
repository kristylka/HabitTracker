namespace HabitTracker.Desktop.Tests.ViewModels;

using CommunityToolkit.Mvvm.Messaging;
using FluentAssertions;
using HabitTracker.Core.Interfaces;
using HabitTracker.Core.Messages;
using HabitTracker.Core.Models;
using HabitTracker.Desktop.ViewModels;
using Moq;

public class MainViewModelTests
{
    private readonly IMessenger _messenger;
    private readonly Mock<INotificationScheduler> _schedulerMock;
    private readonly LoginViewModel _loginVm;
    private readonly RegisterViewModel _registerVm;
    private readonly CalendarViewModel _calendarVm;
    private readonly AnalyticsViewModel _analyticsVm;
    private readonly ProfileViewModel _profileVm;
    private readonly MainViewModel _vm;

    public MainViewModelTests()
    {
        _messenger = new WeakReferenceMessenger();
        _schedulerMock = new Mock<INotificationScheduler>();

        var authMock = new Mock<IAuthService>();
        var habitMock = new Mock<IHabitService>();
        var analyticsMock = new Mock<IAnalyticsService>();
        var notifMock = new Mock<INotificationService>();

        habitMock.Setup(h => h.GetEntriesForDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<HabitEntry>());
        habitMock.Setup(h => h.GetUserHabitsAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Habit>());
        analyticsMock.Setup(a => a.GetCompletionRateAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(0);
        analyticsMock.Setup(a => a.GetHabitCompletionRatesAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new Dictionary<string, double>());
        analyticsMock.Setup(a => a.GetDailyCompletionCountsAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new Dictionary<DateTime, int>());

        _loginVm = new LoginViewModel(authMock.Object, _messenger);
        _registerVm = new RegisterViewModel(authMock.Object, _messenger);
        _calendarVm = new CalendarViewModel(habitMock.Object, _messenger);
        _analyticsVm = new AnalyticsViewModel(analyticsMock.Object, habitMock.Object);
        _profileVm = new ProfileViewModel(_messenger, notifMock.Object);

        _vm = new MainViewModel(
            _messenger, _schedulerMock.Object,
            _loginVm, _registerVm, _calendarVm, _analyticsVm, _profileVm);
    }

    [Fact]
    public void NewViewModel_ShouldStartOnLoginView()
    {
        _vm.CurrentView.Should().Be(_loginVm);
        _vm.IsLoggedIn.Should().BeFalse();
    }

    [Fact]
    public void Receive_UserLoggedIn_NavigatesToCalendarAndStartsScheduler()
    {
        var user = new User { Id = 5, Username = "test", DisplayName = "Test" };

        _vm.Receive(new UserLoggedInMessage(user));

        _vm.IsLoggedIn.Should().BeTrue();
        _vm.CurrentView.Should().Be(_calendarVm);
        _vm.CurrentViewName.Should().Be("Calendar");
        _schedulerMock.Verify(s => s.Start(5), Times.Once);
    }

    [Fact]
    public void Receive_UserLoggedOut_ReturnsToLoginAndStopsScheduler()
    {
        _vm.Receive(new UserLoggedInMessage(new User { Id = 1 }));

        _vm.Receive(new UserLoggedOutMessage());

        _vm.IsLoggedIn.Should().BeFalse();
        _vm.CurrentView.Should().Be(_loginVm);
        _vm.CurrentViewName.Should().Be("Login");
        _schedulerMock.Verify(s => s.Stop(), Times.Once);
    }

    [Fact]
    public void Receive_NavigationToRegister_SwitchesView()
    {
        _vm.Receive(new NavigationMessage("Register"));

        _vm.CurrentView.Should().Be(_registerVm);
    }

    [Fact]
    public void Receive_NavigationToLogin_SwitchesView()
    {
        _vm.Receive(new NavigationMessage("Login"));

        _vm.CurrentView.Should().Be(_loginVm);
    }

    [Fact]
    public void Receive_NavigationToCalendar_SwitchesView()
    {
        _vm.Receive(new UserLoggedInMessage(new User { Id = 1 }));
        _vm.Receive(new NavigationMessage("Analytics"));

        _vm.Receive(new NavigationMessage("Calendar"));

        _vm.CurrentView.Should().Be(_calendarVm);
        _vm.CurrentViewName.Should().Be("Calendar");
    }

    [Fact]
    public void Receive_NavigationToAnalytics_SwitchesView()
    {
        _vm.Receive(new UserLoggedInMessage(new User { Id = 1 }));

        _vm.Receive(new NavigationMessage("Analytics"));

        _vm.CurrentView.Should().Be(_analyticsVm);
        _vm.CurrentViewName.Should().Be("Analytics");
    }

    [Fact]
    public void Receive_NavigationToProfile_SwitchesView()
    {
        _vm.Receive(new UserLoggedInMessage(new User { Id = 1 }));

        _vm.Receive(new NavigationMessage("Profile"));

        _vm.CurrentView.Should().Be(_profileVm);
        _vm.CurrentViewName.Should().Be("Profile");
    }

    [Fact]
    public void Receive_UnknownNavigation_DoesNothing()
    {
        _vm.Receive(new UserLoggedInMessage(new User { Id = 1 }));
        var current = _vm.CurrentView;

        _vm.Receive(new NavigationMessage("UnknownView"));

        _vm.CurrentView.Should().Be(current);
    }

    [Fact]
    public void LogoutCommand_SendsUserLoggedOutMessage()
    {
        _vm.Receive(new UserLoggedInMessage(new User { Id = 1 }));

        _vm.LogoutCommand.Execute(null);

        _vm.IsLoggedIn.Should().BeFalse();
    }

    [Fact]
    public void NavigateCommands_ChangeCurrentView()
    {
        _vm.Receive(new UserLoggedInMessage(new User { Id = 1 }));

        _vm.NavigateToAnalyticsCommand.Execute(null);
        _vm.CurrentView.Should().Be(_analyticsVm);

        _vm.NavigateToProfileCommand.Execute(null);
        _vm.CurrentView.Should().Be(_profileVm);

        _vm.NavigateToCalendarCommand.Execute(null);
        _vm.CurrentView.Should().Be(_calendarVm);
    }
}