namespace HabitTracker.Core.Tests.Models;

using FluentAssertions;
using HabitTracker.Core.Models;

public class HabitTests
{
    [Fact]
    public void NewHabit_ShouldHaveDefaultValues()
    {
        var habit = new Habit();

        habit.Id.Should().Be(0);
        habit.UserId.Should().Be(0);
        habit.Name.Should().BeEmpty();
        habit.Description.Should().BeEmpty();
        habit.Color.Should().Be("#4CAF50");
        habit.IsActive.Should().BeTrue();
        habit.Entries.Should().NotBeNull().And.BeEmpty();
        habit.Schedules.Should().NotBeNull().And.BeEmpty();
        habit.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Habit_ShouldSetPropertiesCorrectly()
    {
        var habit = new Habit
        {
            Id = 1,
            UserId = 10,
            Name = "Drink Water",
            Description = "Daily water",
            Color = "#FF0000",
            IsActive = false
        };

        habit.Id.Should().Be(1);
        habit.UserId.Should().Be(10);
        habit.Name.Should().Be("Drink Water");
        habit.Description.Should().Be("Daily water");
        habit.Color.Should().Be("#FF0000");
        habit.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Habit_CanAddEntriesAndSchedules()
    {
        var habit = new Habit();
        habit.Entries.Add(new HabitEntry());
        habit.Schedules.Add(new HabitSchedule());

        habit.Entries.Should().HaveCount(1);
        habit.Schedules.Should().HaveCount(1);
    }
}