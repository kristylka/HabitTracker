namespace HabitTracker.Core.Tests.Models;

using FluentAssertions;
using HabitTracker.Core.Models;

public class HabitScheduleTests
{
    [Fact]
    public void NewHabitSchedule_ShouldHaveDefaultValues()
    {
        var schedule = new HabitSchedule();

        schedule.Id.Should().Be(0);
        schedule.HabitId.Should().Be(0);
        schedule.NotifyEnabled.Should().BeTrue();
    }

    [Fact]
    public void HabitSchedule_ShouldSetPropertiesCorrectly()
    {
        var schedule = new HabitSchedule
        {
            Id = 1,
            HabitId = 5,
            DayOfWeek = DayOfWeek.Monday,
            Time = new TimeSpan(8, 0, 0),
            NotifyEnabled = false
        };

        schedule.Id.Should().Be(1);
        schedule.HabitId.Should().Be(5);
        schedule.DayOfWeek.Should().Be(DayOfWeek.Monday);
        schedule.Time.Should().Be(new TimeSpan(8, 0, 0));
        schedule.NotifyEnabled.Should().BeFalse();
    }

    [Theory]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Tuesday)]
    [InlineData(DayOfWeek.Wednesday)]
    [InlineData(DayOfWeek.Thursday)]
    [InlineData(DayOfWeek.Friday)]
    [InlineData(DayOfWeek.Saturday)]
    [InlineData(DayOfWeek.Sunday)]
    public void HabitSchedule_SupportsAllDaysOfWeek(DayOfWeek day)
    {
        var schedule = new HabitSchedule { DayOfWeek = day };
        schedule.DayOfWeek.Should().Be(day);
    }
}