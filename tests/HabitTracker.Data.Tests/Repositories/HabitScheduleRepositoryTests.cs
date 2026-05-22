namespace HabitTracker.Data.Tests.Repositories;

using FluentAssertions;
using HabitTracker.Core.Models;
using HabitTracker.Data.Repositories;
using HabitTracker.Data.Tests.Helpers;

public class HabitScheduleRepositoryTests
{
    [Fact]
    public async Task CreateAsync_ShouldAddSchedule()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new HabitScheduleRepository(factory);

        var schedule = new HabitSchedule
        {
            HabitId = 1,
            DayOfWeek = DayOfWeek.Monday,
            Time = new TimeSpan(9, 0, 0)
        };

        var created = await repo.CreateAsync(schedule);

        created.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetByHabitIdAsync_ShouldReturnSortedSchedules()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new HabitScheduleRepository(factory);

        await repo.CreateAsync(new HabitSchedule
        {
            HabitId = 1, DayOfWeek = DayOfWeek.Friday, Time = new TimeSpan(10, 0, 0)
        });
        await repo.CreateAsync(new HabitSchedule
        {
            HabitId = 1, DayOfWeek = DayOfWeek.Monday, Time = new TimeSpan(8, 0, 0)
        });
        await repo.CreateAsync(new HabitSchedule
        {
            HabitId = 1, DayOfWeek = DayOfWeek.Monday, Time = new TimeSpan(15, 0, 0)
        });

        var result = await repo.GetByHabitIdAsync(1);

        result.Should().HaveCount(3);
        result[0].DayOfWeek.Should().Be(DayOfWeek.Monday);
        result[0].Time.Should().Be(new TimeSpan(8, 0, 0));
        result[1].DayOfWeek.Should().Be(DayOfWeek.Monday);
        result[1].Time.Should().Be(new TimeSpan(15, 0, 0));
        result[2].DayOfWeek.Should().Be(DayOfWeek.Friday);
    }

    [Fact]
    public async Task GetByUserAndDayAsync_ShouldFilterCorrectly()
    {
        var dbName = Guid.NewGuid().ToString();
        var factory = InMemoryDbHelper.CreateFactory(dbName);

        await using (var db = factory.CreateDbContext())
        {
            db.Habits.Add(new Habit { Id = 1, UserId = 1, Name = "H1", IsActive = true });
            db.Habits.Add(new Habit { Id = 2, UserId = 1, Name = "H2", IsActive = false });
            db.Habits.Add(new Habit { Id = 3, UserId = 2, Name = "H3", IsActive = true });
            await db.SaveChangesAsync();
        }

        var repo = new HabitScheduleRepository(factory);

        await repo.CreateAsync(new HabitSchedule { HabitId = 1, DayOfWeek = DayOfWeek.Monday, Time = TimeSpan.FromHours(9) });
        await repo.CreateAsync(new HabitSchedule { HabitId = 1, DayOfWeek = DayOfWeek.Tuesday, Time = TimeSpan.FromHours(9) });
        await repo.CreateAsync(new HabitSchedule { HabitId = 2, DayOfWeek = DayOfWeek.Monday, Time = TimeSpan.FromHours(9) });
        await repo.CreateAsync(new HabitSchedule { HabitId = 3, DayOfWeek = DayOfWeek.Monday, Time = TimeSpan.FromHours(9) });

        var result = await repo.GetByUserAndDayAsync(1, DayOfWeek.Monday);

        result.Should().HaveCount(1);
        result[0].HabitId.Should().Be(1);
    }

    [Fact]
    public async Task UpdateAsync_ShouldChangeSchedule()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new HabitScheduleRepository(factory);

        var schedule = await repo.CreateAsync(new HabitSchedule
        {
            HabitId = 1, DayOfWeek = DayOfWeek.Monday, Time = TimeSpan.FromHours(9)
        });

        schedule.Time = TimeSpan.FromHours(15);
        await repo.UpdateAsync(schedule);

        var schedules = await repo.GetByHabitIdAsync(1);
        schedules.Single().Time.Should().Be(TimeSpan.FromHours(15));
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveSchedule()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new HabitScheduleRepository(factory);

        var schedule = await repo.CreateAsync(new HabitSchedule
        {
            HabitId = 1, DayOfWeek = DayOfWeek.Monday, Time = TimeSpan.FromHours(9)
        });

        await repo.DeleteAsync(schedule.Id);

        var result = await repo.GetByHabitIdAsync(1);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_NonExistent_ShouldNotThrow()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new HabitScheduleRepository(factory);

        var act = async () => await repo.DeleteAsync(999);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteByHabitIdAsync_ShouldRemoveAllSchedulesOfHabit()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new HabitScheduleRepository(factory);

        await repo.CreateAsync(new HabitSchedule
        {
            HabitId = 1, DayOfWeek = DayOfWeek.Monday, Time = TimeSpan.FromHours(9)
        });
        await repo.CreateAsync(new HabitSchedule
        {
            HabitId = 1, DayOfWeek = DayOfWeek.Tuesday, Time = TimeSpan.FromHours(9)
        });
        await repo.CreateAsync(new HabitSchedule
        {
            HabitId = 2, DayOfWeek = DayOfWeek.Monday, Time = TimeSpan.FromHours(9)
        });

        await repo.DeleteByHabitIdAsync(1);

        (await repo.GetByHabitIdAsync(1)).Should().BeEmpty();
        (await repo.GetByHabitIdAsync(2)).Should().HaveCount(1);
    }
}