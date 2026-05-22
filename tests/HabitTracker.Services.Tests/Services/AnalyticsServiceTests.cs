namespace HabitTracker.Services.Tests.Services;

using FluentAssertions;
using HabitTracker.Core.Interfaces;
using HabitTracker.Core.Models;
using HabitTracker.Services;
using Moq;

public class AnalyticsServiceTests
{
    private readonly Mock<IHabitEntryRepository> _entryRepoMock;
    private readonly Mock<IHabitRepository> _habitRepoMock;
    private readonly AnalyticsService _service;

    public AnalyticsServiceTests()
    {
        _entryRepoMock = new Mock<IHabitEntryRepository>();
        _habitRepoMock = new Mock<IHabitRepository>();
        _service = new AnalyticsService(_entryRepoMock.Object, _habitRepoMock.Object);
    }

    [Fact]
    public async Task GetCompletionRateAsync_NoEntries_ShouldReturnZero()
    {
        _entryRepoMock.Setup(r => r.GetByDateRangeAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<HabitEntry>());

        var rate = await _service.GetCompletionRateAsync(
            1, DateTime.Today.AddDays(-7), DateTime.Today);

        rate.Should().Be(0);
    }

    [Fact]
    public async Task GetCompletionRateAsync_AllCompleted_ShouldReturn100()
    {
        var entries = new List<HabitEntry>
        {
            new() { IsCompleted = true },
            new() { IsCompleted = true },
            new() { IsCompleted = true }
        };
        _entryRepoMock.Setup(r => r.GetByDateRangeAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(entries);

        var rate = await _service.GetCompletionRateAsync(1, DateTime.Today, DateTime.Today);

        rate.Should().Be(100);
    }

    [Fact]
    public async Task GetCompletionRateAsync_HalfCompleted_ShouldReturn50()
    {
        var entries = new List<HabitEntry>
        {
            new() { IsCompleted = true },
            new() { IsCompleted = true },
            new() { IsCompleted = false },
            new() { IsCompleted = false }
        };
        _entryRepoMock.Setup(r => r.GetByDateRangeAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(entries);

        var rate = await _service.GetCompletionRateAsync(1, DateTime.Today, DateTime.Today);

        rate.Should().Be(50);
    }

    [Fact]
    public async Task GetCompletionRateAsync_NoneCompleted_ShouldReturnZero()
    {
        var entries = new List<HabitEntry>
        {
            new() { IsCompleted = false },
            new() { IsCompleted = false }
        };
        _entryRepoMock.Setup(r => r.GetByDateRangeAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(entries);

        var rate = await _service.GetCompletionRateAsync(1, DateTime.Today, DateTime.Today);

        rate.Should().Be(0);
    }

    [Fact]
    public async Task GetCurrentStreakAsync_NoEntries_ShouldReturnZero()
    {
        _entryRepoMock.Setup(r => r.GetByHabitIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<HabitEntry>());

        var streak = await _service.GetCurrentStreakAsync(1);

        streak.Should().Be(0);
    }

    [Fact]
    public async Task GetCurrentStreakAsync_TodayCompleted_ShouldReturnOne()
    {
        var entries = new List<HabitEntry>
        {
            new() { Date = DateTime.Today, IsCompleted = true }
        };
        _entryRepoMock.Setup(r => r.GetByHabitIdAsync(1)).ReturnsAsync(entries);

        var streak = await _service.GetCurrentStreakAsync(1);

        streak.Should().Be(1);
    }

    [Fact]
    public async Task GetCurrentStreakAsync_ThreeConsecutiveDays_ShouldReturnThree()
    {
        var entries = new List<HabitEntry>
        {
            new() { Date = DateTime.Today.AddDays(-2), IsCompleted = true },
            new() { Date = DateTime.Today.AddDays(-1), IsCompleted = true },
            new() { Date = DateTime.Today, IsCompleted = true }
        };
        _entryRepoMock.Setup(r => r.GetByHabitIdAsync(1)).ReturnsAsync(entries);

        var streak = await _service.GetCurrentStreakAsync(1);

        streak.Should().Be(3);
    }

    [Fact]
    public async Task GetCurrentStreakAsync_BrokenStreak_CountsOnlyFromToday()
    {
        var entries = new List<HabitEntry>
        {
            new() { Date = DateTime.Today.AddDays(-3), IsCompleted = true },
            new() { Date = DateTime.Today.AddDays(-2), IsCompleted = false },
            new() { Date = DateTime.Today.AddDays(-1), IsCompleted = true },
            new() { Date = DateTime.Today, IsCompleted = true }
        };
        _entryRepoMock.Setup(r => r.GetByHabitIdAsync(1)).ReturnsAsync(entries);

        var streak = await _service.GetCurrentStreakAsync(1);

        streak.Should().Be(2);
    }

    [Fact]
    public async Task GetCurrentStreakAsync_TodayNotCompleted_ShouldReturnZero()
    {
        var entries = new List<HabitEntry>
        {
            new() { Date = DateTime.Today.AddDays(-1), IsCompleted = true },
            new() { Date = DateTime.Today, IsCompleted = false }
        };
        _entryRepoMock.Setup(r => r.GetByHabitIdAsync(1)).ReturnsAsync(entries);

        var streak = await _service.GetCurrentStreakAsync(1);

        streak.Should().Be(0);
    }

    [Fact]
    public async Task GetCurrentStreakAsync_IgnoresFutureEntries()
    {
        var entries = new List<HabitEntry>
        {
            new() { Date = DateTime.Today, IsCompleted = true },
            new() { Date = DateTime.Today.AddDays(1), IsCompleted = false }
        };
        _entryRepoMock.Setup(r => r.GetByHabitIdAsync(1)).ReturnsAsync(entries);

        var streak = await _service.GetCurrentStreakAsync(1);

        streak.Should().Be(1);
    }

    [Fact]
    public async Task GetBestStreakAsync_NoEntries_ShouldReturnZero()
    {
        _entryRepoMock.Setup(r => r.GetByHabitIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<HabitEntry>());

        var best = await _service.GetBestStreakAsync(1);

        best.Should().Be(0);
    }

    [Fact]
    public async Task GetBestStreakAsync_AllCompleted_ShouldReturnTotal()
    {
        var entries = new List<HabitEntry>
        {
            new() { Date = DateTime.Today.AddDays(-2), IsCompleted = true },
            new() { Date = DateTime.Today.AddDays(-1), IsCompleted = true },
            new() { Date = DateTime.Today, IsCompleted = true }
        };
        _entryRepoMock.Setup(r => r.GetByHabitIdAsync(1)).ReturnsAsync(entries);

        var best = await _service.GetBestStreakAsync(1);

        best.Should().Be(3);
    }

    [Fact]
    public async Task GetBestStreakAsync_MultipleStreaks_ReturnsLongest()
    {
        var entries = new List<HabitEntry>
        {
            new() { Date = DateTime.Today.AddDays(-7), IsCompleted = true },
            new() { Date = DateTime.Today.AddDays(-6), IsCompleted = true },
            new() { Date = DateTime.Today.AddDays(-5), IsCompleted = true },
            new() { Date = DateTime.Today.AddDays(-4), IsCompleted = true },
            new() { Date = DateTime.Today.AddDays(-3), IsCompleted = false },
            new() { Date = DateTime.Today.AddDays(-2), IsCompleted = true },
            new() { Date = DateTime.Today.AddDays(-1), IsCompleted = true },
            new() { Date = DateTime.Today, IsCompleted = false }
        };
        _entryRepoMock.Setup(r => r.GetByHabitIdAsync(1)).ReturnsAsync(entries);

        var best = await _service.GetBestStreakAsync(1);

        best.Should().Be(4);
    }

    [Fact]
    public async Task GetBestStreakAsync_AllFailed_ShouldReturnZero()
    {
        var entries = new List<HabitEntry>
        {
            new() { Date = DateTime.Today.AddDays(-1), IsCompleted = false },
            new() { Date = DateTime.Today, IsCompleted = false }
        };
        _entryRepoMock.Setup(r => r.GetByHabitIdAsync(1)).ReturnsAsync(entries);

        var best = await _service.GetBestStreakAsync(1);

        best.Should().Be(0);
    }

    [Fact]
    public async Task GetHabitCompletionRatesAsync_NoEntries_ShouldReturnEmpty()
    {
        _entryRepoMock.Setup(r => r.GetByDateRangeAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<HabitEntry>());

        var result = await _service.GetHabitCompletionRatesAsync(
            1, DateTime.Today, DateTime.Today);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHabitCompletionRatesAsync_GroupsByHabitName()
    {
        var water = new Habit { Name = "Water" };
        var read = new Habit { Name = "Read" };

        var entries = new List<HabitEntry>
        {
            new() { Habit = water, IsCompleted = true },
            new() { Habit = water, IsCompleted = true },
            new() { Habit = water, IsCompleted = false },
            new() { Habit = read, IsCompleted = true },
            new() { Habit = read, IsCompleted = false }
        };
        _entryRepoMock.Setup(r => r.GetByDateRangeAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(entries);

        var result = await _service.GetHabitCompletionRatesAsync(
            1, DateTime.Today, DateTime.Today);

        result.Should().HaveCount(2);
        result["Water"].Should().BeApproximately(66.67, 0.1);
        result["Read"].Should().Be(50);
    }

    [Fact]
    public async Task GetHabitCompletionRatesAsync_NullHabit_UsesUnknown()
    {
        var entries = new List<HabitEntry>
        {
            new() { Habit = null, IsCompleted = true }
        };
        _entryRepoMock.Setup(r => r.GetByDateRangeAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(entries);

        var result = await _service.GetHabitCompletionRatesAsync(
            1, DateTime.Today, DateTime.Today);

        result.Should().ContainKey("Unknown");
        result["Unknown"].Should().Be(100);
    }

    [Fact]
    public async Task GetDailyCompletionCountsAsync_NoEntries_ShouldReturnEmpty()
    {
        _entryRepoMock.Setup(r => r.GetByDateRangeAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<HabitEntry>());

        var result = await _service.GetDailyCompletionCountsAsync(
            1, DateTime.Today, DateTime.Today);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDailyCompletionCountsAsync_OnlyCompleted_AreCounted()
    {
        var day = new DateTime(2024, 6, 15);
        var entries = new List<HabitEntry>
        {
            new() { Date = day, IsCompleted = true },
            new() { Date = day, IsCompleted = true },
            new() { Date = day, IsCompleted = false }
        };
        _entryRepoMock.Setup(r => r.GetByDateRangeAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(entries);

        var result = await _service.GetDailyCompletionCountsAsync(
            1, day, day);

        result.Should().HaveCount(1);
        result[day].Should().Be(2);
    }

    [Fact]
    public async Task GetDailyCompletionCountsAsync_GroupsByDate()
    {
        var day1 = new DateTime(2024, 6, 15);
        var day2 = new DateTime(2024, 6, 16);
        var entries = new List<HabitEntry>
        {
            new() { Date = day1, IsCompleted = true },
            new() { Date = day1, IsCompleted = true },
            new() { Date = day2, IsCompleted = true }
        };
        _entryRepoMock.Setup(r => r.GetByDateRangeAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(entries);

        var result = await _service.GetDailyCompletionCountsAsync(
            1, day1, day2);

        result.Should().HaveCount(2);
        result[day1].Should().Be(2);
        result[day2].Should().Be(1);
    }
}