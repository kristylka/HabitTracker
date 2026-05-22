namespace HabitTracker.Services.Tests.Services;

using FluentAssertions;
using HabitTracker.Core.Interfaces;
using HabitTracker.Core.Models;
using HabitTracker.Services;
using Moq;

public class HabitServiceTests
{
    private readonly Mock<IHabitRepository> _habitRepoMock;
    private readonly Mock<IHabitEntryRepository> _entryRepoMock;
    private readonly Mock<IHabitScheduleRepository> _scheduleRepoMock;
    private readonly HabitService _service;

    public HabitServiceTests()
    {
        _habitRepoMock = new Mock<IHabitRepository>();
        _entryRepoMock = new Mock<IHabitEntryRepository>();
        _scheduleRepoMock = new Mock<IHabitScheduleRepository>();
        _service = new HabitService(
            _habitRepoMock.Object,
            _entryRepoMock.Object,
            _scheduleRepoMock.Object);
    }

    [Fact]
    public async Task GetUserHabitsAsync_ShouldReturnHabitsFromRepo()
    {
        var habits = new List<Habit>
        {
            new() { Id = 1, Name = "Water", UserId = 1 },
            new() { Id = 2, Name = "Read", UserId = 1 }
        };
        _habitRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(habits);

        var result = await _service.GetUserHabitsAsync(1);

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Water");
        _habitRepoMock.Verify(r => r.GetByUserIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetUserHabitsAsync_EmptyList_ShouldReturnEmpty()
    {
        _habitRepoMock.Setup(r => r.GetByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Habit>());

        var result = await _service.GetUserHabitsAsync(99);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateHabitAsync_ValidData_ShouldCreateHabitAndSchedules()
    {
        _habitRepoMock.Setup(r => r.CreateAsync(It.IsAny<Habit>()))
            .ReturnsAsync((Habit h) => { h.Id = 10; return h; });
        _scheduleRepoMock.Setup(r => r.CreateAsync(It.IsAny<HabitSchedule>()))
            .ReturnsAsync((HabitSchedule s) => s);

        var schedules = new List<(DayOfWeek, TimeSpan, bool)>
        {
            (DayOfWeek.Monday, new TimeSpan(9, 0, 0), true),
            (DayOfWeek.Friday, new TimeSpan(10, 0, 0), false)
        };

        var habit = await _service.CreateHabitAsync(
            userId: 1,
            name: "Drink Water",
            description: "Stay hydrated",
            color: "#FF0000",
            schedules: schedules);

        habit.Id.Should().Be(10);
        habit.Name.Should().Be("Drink Water");
        habit.Description.Should().Be("Stay hydrated");
        habit.Color.Should().Be("#FF0000");
        habit.UserId.Should().Be(1);
        habit.IsActive.Should().BeTrue();

        _habitRepoMock.Verify(r => r.CreateAsync(It.IsAny<Habit>()), Times.Once);
        _scheduleRepoMock.Verify(r => r.CreateAsync(It.IsAny<HabitSchedule>()), Times.Exactly(2));
    }

    [Fact]
    public async Task CreateHabitAsync_NoSchedules_ShouldStillCreateHabit()
    {
        _habitRepoMock.Setup(r => r.CreateAsync(It.IsAny<Habit>()))
            .ReturnsAsync((Habit h) => h);

        var habit = await _service.CreateHabitAsync(
            1, "Test", "", "#000000",
            new List<(DayOfWeek, TimeSpan, bool)>());

        habit.Should().NotBeNull();
        _habitRepoMock.Verify(r => r.CreateAsync(It.IsAny<Habit>()), Times.Once);
        _scheduleRepoMock.Verify(r => r.CreateAsync(It.IsAny<HabitSchedule>()), Times.Never);
    }

    [Fact]
    public async Task CreateHabitAsync_EmptyName_ShouldThrow()
    {
        var act = async () => await _service.CreateHabitAsync(
            1, "", "desc", "#000000", new List<(DayOfWeek, TimeSpan, bool)>());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*пустым*");
    }

    [Fact]
    public async Task CreateHabitAsync_WhitespaceName_ShouldThrow()
    {
        var act = async () => await _service.CreateHabitAsync(
            1, "   ", "desc", "#000000", new List<(DayOfWeek, TimeSpan, bool)>());

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateHabitAsync_NullDescription_ShouldUseEmptyString()
    {
        _habitRepoMock.Setup(r => r.CreateAsync(It.IsAny<Habit>()))
            .ReturnsAsync((Habit h) => h);

        var habit = await _service.CreateHabitAsync(
            1, "Test", null!, "#000000",
            new List<(DayOfWeek, TimeSpan, bool)>());

        habit.Description.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateHabitAsync_ShouldTrimName()
    {
        _habitRepoMock.Setup(r => r.CreateAsync(It.IsAny<Habit>()))
            .ReturnsAsync((Habit h) => h);

        var habit = await _service.CreateHabitAsync(
            1, "  Walking  ", "", "#000",
            new List<(DayOfWeek, TimeSpan, bool)>());

        habit.Name.Should().Be("Walking");
    }

    [Fact]
    public async Task UpdateHabitAsync_ShouldCallRepo()
    {
        var habit = new Habit { Id = 5, Name = "Updated" };

        await _service.UpdateHabitAsync(habit);

        _habitRepoMock.Verify(r => r.UpdateAsync(habit), Times.Once);
    }

    [Fact]
    public async Task DeleteHabitAsync_ShouldCallRepo()
    {
        await _service.DeleteHabitAsync(42);

        _habitRepoMock.Verify(r => r.DeleteAsync(42), Times.Once);
    }

    [Fact]
    public async Task GenerateEntriesForDateAsync_CreatesEntriesForSchedules()
    {
        var date = new DateTime(2024, 6, 17); // Monday
        var schedules = new List<HabitSchedule>
        {
            new() { HabitId = 1, DayOfWeek = DayOfWeek.Monday, Time = new TimeSpan(9, 0, 0) },
            new() { HabitId = 2, DayOfWeek = DayOfWeek.Monday, Time = new TimeSpan(20, 0, 0) }
        };

        _scheduleRepoMock.Setup(r => r.GetByUserAndDayAsync(1, DayOfWeek.Monday))
            .ReturnsAsync(schedules);
        _entryRepoMock.Setup(r => r.GetByHabitAndDateAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync((HabitEntry?)null);
        _entryRepoMock.Setup(r => r.CreateAsync(It.IsAny<HabitEntry>()))
            .ReturnsAsync((HabitEntry e) => e);

        await _service.GenerateEntriesForDateAsync(1, date);

        _entryRepoMock.Verify(r => r.CreateAsync(It.IsAny<HabitEntry>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GenerateEntriesForDateAsync_ExistingEntry_DoesNotDuplicate()
    {
        var date = new DateTime(2024, 6, 17);
        var schedules = new List<HabitSchedule>
        {
            new() { HabitId = 1, DayOfWeek = DayOfWeek.Monday, Time = new TimeSpan(9, 0, 0) }
        };
        var existing = new HabitEntry { Id = 1, HabitId = 1 };

        _scheduleRepoMock.Setup(r => r.GetByUserAndDayAsync(1, DayOfWeek.Monday))
            .ReturnsAsync(schedules);
        _entryRepoMock.Setup(r => r.GetByHabitAndDateAsync(
                1, date, new TimeSpan(9, 0, 0)))
            .ReturnsAsync(existing);

        await _service.GenerateEntriesForDateAsync(1, date);

        _entryRepoMock.Verify(r => r.CreateAsync(It.IsAny<HabitEntry>()), Times.Never);
    }

    [Fact]
    public async Task GenerateEntriesForDateAsync_NoSchedules_DoesNothing()
    {
        _scheduleRepoMock.Setup(r => r.GetByUserAndDayAsync(It.IsAny<int>(), It.IsAny<DayOfWeek>()))
            .ReturnsAsync(new List<HabitSchedule>());

        await _service.GenerateEntriesForDateAsync(1, DateTime.Today);

        _entryRepoMock.Verify(r => r.CreateAsync(It.IsAny<HabitEntry>()), Times.Never);
    }

    [Fact]
    public async Task GetEntriesForDateAsync_ShouldGenerateAndReturnEntries()
    {
        var date = DateTime.Today;
        var entries = new List<HabitEntry> { new() { Id = 1 } };

        _scheduleRepoMock.Setup(r => r.GetByUserAndDayAsync(It.IsAny<int>(), It.IsAny<DayOfWeek>()))
            .ReturnsAsync(new List<HabitSchedule>());
        _entryRepoMock.Setup(r => r.GetByDateAsync(1, date)).ReturnsAsync(entries);

        var result = await _service.GetEntriesForDateAsync(1, date);

        result.Should().HaveCount(1);
        _entryRepoMock.Verify(r => r.GetByDateAsync(1, date), Times.Once);
    }

    [Fact]
    public async Task GetEntriesForRangeAsync_ShouldReturnEntriesFromRepo()
    {
        var from = new DateTime(2024, 6, 1);
        var to = new DateTime(2024, 6, 30);
        var entries = new List<HabitEntry> { new() { Id = 1 }, new() { Id = 2 } };

        _entryRepoMock.Setup(r => r.GetByDateRangeAsync(1, from, to)).ReturnsAsync(entries);

        var result = await _service.GetEntriesForRangeAsync(1, from, to);

        result.Should().HaveCount(2);
        _entryRepoMock.Verify(r => r.GetByDateRangeAsync(1, from, to), Times.Once);
    }

    [Fact]
    public async Task ToggleEntryCompletionAsync_NotCompleted_BecomesCompleted()
    {
        var entry = new HabitEntry { Id = 1, IsCompleted = false };
        _entryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entry);

        await _service.ToggleEntryCompletionAsync(1);

        entry.IsCompleted.Should().BeTrue();
        entry.CompletedAt.Should().NotBeNull();
        entry.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        _entryRepoMock.Verify(r => r.UpdateAsync(entry), Times.Once);
    }

    [Fact]
    public async Task ToggleEntryCompletionAsync_Completed_BecomesNotCompleted()
    {
        var entry = new HabitEntry
        {
            Id = 1,
            IsCompleted = true,
            CompletedAt = DateTime.UtcNow.AddHours(-1)
        };
        _entryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entry);

        await _service.ToggleEntryCompletionAsync(1);

        entry.IsCompleted.Should().BeFalse();
        entry.CompletedAt.Should().BeNull();
        _entryRepoMock.Verify(r => r.UpdateAsync(entry), Times.Once);
    }

    [Fact]
    public async Task ToggleEntryCompletionAsync_NotFound_ShouldThrow()
    {
        _entryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((HabitEntry?)null);

        var act = async () => await _service.ToggleEntryCompletionAsync(99);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*не найдена*");
    }
}