namespace HabitTracker.Desktop.Tests.ViewModels;

using FluentAssertions;
using HabitTracker.Core.Interfaces;
using HabitTracker.Core.Models;
using HabitTracker.Desktop.ViewModels;
using Moq;

public class AnalyticsViewModelTests
{
    private readonly Mock<IAnalyticsService> _analyticsMock;
    private readonly Mock<IHabitService> _habitMock;
    private readonly AnalyticsViewModel _vm;

    public AnalyticsViewModelTests()
    {
        _analyticsMock = new Mock<IAnalyticsService>();
        _habitMock = new Mock<IHabitService>();
        _vm = new AnalyticsViewModel(_analyticsMock.Object, _habitMock.Object);
    }

    [Fact]
    public void NewViewModel_ShouldHaveDefaultValues()
    {
        _vm.SelectedPeriodDays.Should().Be(7);
        _vm.OverallCompletionRate.Should().Be(0);
        _vm.HabitStats.Should().BeEmpty();
        _vm.DailyStats.Should().BeEmpty();
        _vm.IsLoading.Should().BeFalse();
        _vm.HasNoData.Should().BeFalse();
        _vm.HasData.Should().BeTrue();
    }

    [Fact]
    public void HasData_IsOppositeOfHasNoData()
    {
        _vm.HasNoData = true;
        _vm.HasData.Should().BeFalse();

        _vm.HasNoData = false;
        _vm.HasData.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAnalytics_WithoutUser_DoesNothing()
    {
        await _vm.LoadAnalyticsCommand.ExecuteAsync(null);

        _analyticsMock.Verify(a => a.GetCompletionRateAsync(
            It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()),
            Times.Never);
    }

    [Fact]
    public async Task LoadAnalytics_NoHabits_SetsHasNoData()
    {
        _vm.SetUser(1);

        _analyticsMock.Setup(a => a.GetCompletionRateAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(0);
        _analyticsMock.Setup(a => a.GetHabitCompletionRatesAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new Dictionary<string, double>());
        _analyticsMock.Setup(a => a.GetDailyCompletionCountsAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new Dictionary<DateTime, int>());
        _habitMock.Setup(h => h.GetUserHabitsAsync(1)).ReturnsAsync(new List<Habit>());

        await _vm.LoadAnalyticsCommand.ExecuteAsync(null);

        _vm.HasNoData.Should().BeTrue();
        _vm.HasData.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAnalytics_WithHabits_PopulatesStats()
    {
        _vm.SetUser(1);

        var habits = new List<Habit>
        {
            new() { Id = 1, Name = "Water", Color = "#FF0000" }
        };
        var rates = new Dictionary<string, double> { ["Water"] = 75.0 };
        var daily = new Dictionary<DateTime, int> { [DateTime.Today] = 3 };

        _analyticsMock.Setup(a => a.GetCompletionRateAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(75.0);
        _analyticsMock.Setup(a => a.GetHabitCompletionRatesAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(rates);
        _analyticsMock.Setup(a => a.GetDailyCompletionCountsAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(daily);
        _analyticsMock.Setup(a => a.GetCurrentStreakAsync(1)).ReturnsAsync(5);
        _analyticsMock.Setup(a => a.GetBestStreakAsync(1)).ReturnsAsync(10);
        _habitMock.Setup(h => h.GetUserHabitsAsync(1)).ReturnsAsync(habits);

        await _vm.LoadAnalyticsCommand.ExecuteAsync(null);

        _vm.OverallCompletionRate.Should().Be(75.0);
        _vm.HabitStats.Should().HaveCount(1);
        _vm.HabitStats[0].Name.Should().Be("Water");
        _vm.HabitStats[0].CompletionRate.Should().Be(75.0);
        _vm.HabitStats[0].CurrentStreak.Should().Be(5);
        _vm.HabitStats[0].BestStreak.Should().Be(10);
        _vm.DailyStats.Should().HaveCount(7);
        _vm.HasNoData.Should().BeFalse();
    }

    [Fact]
    public async Task SetPeriodAsync_ValidNumber_ChangesPeriodAndReloads()
    {
        _vm.SetUser(1);

        _analyticsMock.Setup(a => a.GetCompletionRateAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(0);
        _analyticsMock.Setup(a => a.GetHabitCompletionRatesAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new Dictionary<string, double>());
        _analyticsMock.Setup(a => a.GetDailyCompletionCountsAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new Dictionary<DateTime, int>());
        _habitMock.Setup(h => h.GetUserHabitsAsync(It.IsAny<int>())).ReturnsAsync(new List<Habit>());

        await _vm.SetPeriodCommand.ExecuteAsync("30");

        _vm.SelectedPeriodDays.Should().Be(30);
    }

    [Fact]
    public async Task SetPeriodAsync_InvalidNumber_DoesNothing()
    {
        await _vm.SetPeriodCommand.ExecuteAsync("abc");

        _vm.SelectedPeriodDays.Should().Be(7);
        _analyticsMock.Verify(a => a.GetCompletionRateAsync(
            It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()),
            Times.Never);
    }

    [Fact]
    public void HabitStatItem_DefaultValues()
    {
        var item = new HabitStatItem();

        item.Name.Should().BeEmpty();
        item.Color.Should().Be("#4CAF50");
        item.CompletionRate.Should().Be(0);
        item.CurrentStreak.Should().Be(0);
        item.BestStreak.Should().Be(0);
    }

    [Fact]
    public void DailyStatItem_CanSetProperties()
    {
        var item = new DailyStatItem
        {
            Date = new DateTime(2024, 6, 15),
            Count = 3
        };

        item.Date.Should().Be(new DateTime(2024, 6, 15));
        item.Count.Should().Be(3);
    }
}