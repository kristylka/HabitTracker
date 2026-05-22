namespace HabitTracker.Desktop.Tests.ViewModels;

using CommunityToolkit.Mvvm.Messaging;
using FluentAssertions;
using HabitTracker.Core.Interfaces;
using HabitTracker.Core.Messages;
using HabitTracker.Core.Models;
using HabitTracker.Desktop.ViewModels;
using Moq;

public class CalendarViewModelTests
{
    private readonly Mock<IHabitService> _habitMock;
    private readonly IMessenger _messenger;
    private readonly CalendarViewModel _vm;

    public CalendarViewModelTests()
    {
        _habitMock = new Mock<IHabitService>();
        _messenger = new WeakReferenceMessenger();
        _vm = new CalendarViewModel(_habitMock.Object, _messenger);
    }

    [Fact]
    public void NewViewModel_ShouldHaveDefaultValues()
    {
        _vm.SelectedDate.Should().Be(DateTime.Today);
        _vm.Entries.Should().BeEmpty();
        _vm.Habits.Should().BeEmpty();
        _vm.IsAddHabitVisible.Should().BeFalse();
        _vm.NewHabitName.Should().BeEmpty();
        _vm.NewHabitColor.Should().Be("#4CAF50");
        _vm.Monday.Should().BeTrue();
        _vm.Saturday.Should().BeFalse();
    }

    [Fact]
    public async Task LoadEntries_WithoutUser_DoesNothing()
    {
        await _vm.LoadEntriesCommand.ExecuteAsync(null);

        _habitMock.Verify(h => h.GetEntriesForDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()),
            Times.Never);
    }

    [Fact]
    public async Task LoadEntries_WithUser_LoadsEntriesAndHabits()
    {
        _vm.SetUser(1);

        var entries = new List<HabitEntry> { new() { Id = 1 } };
        var habits = new List<Habit> { new() { Id = 1, Name = "Water" } };

        _habitMock.Setup(h => h.GetEntriesForDateAsync(1, It.IsAny<DateTime>()))
            .ReturnsAsync(entries);
        _habitMock.Setup(h => h.GetUserHabitsAsync(1)).ReturnsAsync(habits);

        await _vm.LoadEntriesCommand.ExecuteAsync(null);

        _vm.Entries.Should().HaveCount(1);
        _vm.Habits.Should().HaveCount(1);
    }

    [Fact]
    public async Task ToggleEntry_ShouldCallServiceAndReload()
    {
        _vm.SetUser(1);
        var entry = new HabitEntry { Id = 5 };

        _habitMock.Setup(h => h.GetEntriesForDateAsync(1, It.IsAny<DateTime>()))
            .ReturnsAsync(new List<HabitEntry>());
        _habitMock.Setup(h => h.GetUserHabitsAsync(1)).ReturnsAsync(new List<Habit>());

        await _vm.ToggleEntryCommand.ExecuteAsync(entry);

        _habitMock.Verify(h => h.ToggleEntryCompletionAsync(5), Times.Once);
        _habitMock.Verify(h => h.GetEntriesForDateAsync(1, It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task PreviousDayCommand_DecrementsDateByOne()
    {
        _vm.SetUser(1);
        _habitMock.Setup(h => h.GetEntriesForDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<HabitEntry>());
        _habitMock.Setup(h => h.GetUserHabitsAsync(It.IsAny<int>())).ReturnsAsync(new List<Habit>());

        var startDate = _vm.SelectedDate;
        await _vm.PreviousDayCommand.ExecuteAsync(null);

        _vm.SelectedDate.Should().Be(startDate.AddDays(-1));
    }

    [Fact]
    public async Task NextDayCommand_IncrementsDateByOne()
    {
        _vm.SetUser(1);
        _habitMock.Setup(h => h.GetEntriesForDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<HabitEntry>());
        _habitMock.Setup(h => h.GetUserHabitsAsync(It.IsAny<int>())).ReturnsAsync(new List<Habit>());

        var startDate = _vm.SelectedDate;
        await _vm.NextDayCommand.ExecuteAsync(null);

        _vm.SelectedDate.Should().Be(startDate.AddDays(1));
    }

    [Fact]
    public async Task GoToTodayCommand_ResetsDateToToday()
    {
        _vm.SetUser(1);
        _vm.SelectedDate = DateTime.Today.AddDays(-10);

        _habitMock.Setup(h => h.GetEntriesForDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<HabitEntry>());
        _habitMock.Setup(h => h.GetUserHabitsAsync(It.IsAny<int>())).ReturnsAsync(new List<Habit>());

        await _vm.GoToTodayCommand.ExecuteAsync(null);

        _vm.SelectedDate.Should().Be(DateTime.Today);
    }

    [Fact]
    public void ShowAddHabitCommand_TogglesVisibility()
    {
        _vm.IsAddHabitVisible.Should().BeFalse();

        _vm.ShowAddHabitCommand.Execute(null);
        _vm.IsAddHabitVisible.Should().BeTrue();

        _vm.ShowAddHabitCommand.Execute(null);
        _vm.IsAddHabitVisible.Should().BeFalse();
    }

    [Fact]
    public async Task AddHabitCommand_EmptyName_DoesNothing()
    {
        _vm.SetUser(1);
        _vm.NewHabitName = "";

        await _vm.AddHabitCommand.ExecuteAsync(null);

        _habitMock.Verify(h => h.CreateHabitAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<List<(DayOfWeek, TimeSpan, bool)>>()),
            Times.Never);
    }

    [Fact]
    public async Task AddHabitCommand_ValidData_CreatesHabitAndResetsForm()
    {
        _vm.SetUser(1);
        _vm.NewHabitName = "Water";
        _vm.NewHabitDescription = "Drink water";
        _vm.NewHabitColor = "#FF0000";
        _vm.NewHabitTime = new TimeSpan(9, 0, 0);
        _vm.Monday = true;
        _vm.Tuesday = false;
        _vm.Wednesday = false;
        _vm.Thursday = false;
        _vm.Friday = false;
        _vm.Saturday = false;
        _vm.Sunday = false;
        _vm.IsAddHabitVisible = true;

        _habitMock.Setup(h => h.CreateHabitAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<List<(DayOfWeek, TimeSpan, bool)>>()))
            .ReturnsAsync(new Habit());

        await _vm.AddHabitCommand.ExecuteAsync(null);

        _habitMock.Verify(h => h.CreateHabitAsync(
                1, "Water", "Drink water", "#FF0000",
                It.Is<List<(DayOfWeek, TimeSpan, bool)>>(l => l.Count == 1 && l[0].Item1 == DayOfWeek.Monday)),
            Times.Once);

        _vm.NewHabitName.Should().BeEmpty();
        _vm.NewHabitDescription.Should().BeEmpty();
        _vm.IsAddHabitVisible.Should().BeFalse();
    }

    [Fact]
    public async Task AddHabitCommand_AllDaysSelected_CreatesAllSchedules()
    {
        _vm.SetUser(1);
        _vm.NewHabitName = "Daily";
        _vm.Monday = _vm.Tuesday = _vm.Wednesday = _vm.Thursday = true;
        _vm.Friday = _vm.Saturday = _vm.Sunday = true;

        _habitMock.Setup(h => h.CreateHabitAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<List<(DayOfWeek, TimeSpan, bool)>>()))
            .ReturnsAsync(new Habit());

        await _vm.AddHabitCommand.ExecuteAsync(null);

        _habitMock.Verify(h => h.CreateHabitAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.Is<List<(DayOfWeek, TimeSpan, bool)>>(l => l.Count == 7)),
            Times.Once);
    }

    [Fact]
    public async Task DeleteHabit_ShouldCallServiceAndSendMessage()
    {
        _vm.SetUser(1);
        var habit = new Habit { Id = 42 };

        var changed = false;
        _messenger.Register<CalendarViewModelTests, HabitChangedMessage>(this, (r, m) => changed = true);

        await _vm.DeleteHabitCommand.ExecuteAsync(habit);

        _habitMock.Verify(h => h.DeleteHabitAsync(42), Times.Once);
        changed.Should().BeTrue();
    }

    [Fact]
    public void Receive_HabitChangedMessage_TriggersReload()
    {
        _vm.SetUser(1);

        _habitMock.Setup(h => h.GetEntriesForDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<HabitEntry>());
        _habitMock.Setup(h => h.GetUserHabitsAsync(It.IsAny<int>())).ReturnsAsync(new List<Habit>());

        _vm.Receive(new HabitChangedMessage());

        System.Threading.Thread.Sleep(100);

        _habitMock.Verify(h => h.GetEntriesForDateAsync(1, It.IsAny<DateTime>()), Times.AtLeastOnce);
    }
}