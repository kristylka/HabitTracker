namespace HabitTracker.Core.Tests.Models;

using FluentAssertions;
using HabitTracker.Core.Models;

public class HabitEntryTests
{
    [Fact]
    public void NewHabitEntry_ShouldHaveDefaultValues()
    {
        var entry = new HabitEntry();

        entry.Id.Should().Be(0);
        entry.HabitId.Should().Be(0);
        entry.IsCompleted.Should().BeFalse();
        entry.CompletedAt.Should().BeNull();
        entry.ScheduledTime.Should().BeNull();
        entry.Note.Should().BeNull();
    }

    [Fact]
    public void HabitEntry_ShouldSetPropertiesCorrectly()
    {
        var date = new DateTime(2024, 6, 15);
        var time = new TimeSpan(9, 30, 0);
        var completed = new DateTime(2024, 6, 15, 10, 0, 0);

        var entry = new HabitEntry
        {
            Id = 1,
            HabitId = 5,
            Date = date,
            ScheduledTime = time,
            IsCompleted = true,
            CompletedAt = completed,
            Note = "Done"
        };

        entry.Id.Should().Be(1);
        entry.HabitId.Should().Be(5);
        entry.Date.Should().Be(date);
        entry.ScheduledTime.Should().Be(time);
        entry.IsCompleted.Should().BeTrue();
        entry.CompletedAt.Should().Be(completed);
        entry.Note.Should().Be("Done");
    }
}