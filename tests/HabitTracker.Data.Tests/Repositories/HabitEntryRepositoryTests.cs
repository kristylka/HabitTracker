namespace HabitTracker.Data.Tests.Repositories;

using FluentAssertions;
using HabitTracker.Core.Models;
using HabitTracker.Data.Repositories;
using HabitTracker.Data.Tests.Helpers;

public class HabitEntryRepositoryTests
{
    [Fact]
    public async Task CreateAsync_ShouldAddEntry()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new HabitEntryRepository(factory);

        var entry = new HabitEntry
        {
            HabitId = 1,
            Date = DateTime.Today,
            IsCompleted = false
        };

        var created = await repo.CreateAsync(entry);

        created.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingEntry_ShouldReturnEntry()
    {
        var dbName = Guid.NewGuid().ToString();
        var factory = InMemoryDbHelper.CreateFactory(dbName);

        await using (var db = factory.CreateDbContext())
        {
            db.Habits.Add(new Habit { Id = 1, UserId = 1, Name = "H", IsActive = true });
            await db.SaveChangesAsync();
        }

        var repo = new HabitEntryRepository(factory);
        var entry = await repo.CreateAsync(new HabitEntry
        {
            HabitId = 1,
            Date = DateTime.Today
        });

        var found = await repo.GetByIdAsync(entry.Id);

        found.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ShouldReturnNull()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new HabitEntryRepository(factory);

        var found = await repo.GetByIdAsync(999);

        found.Should().BeNull();
    }

    [Fact]
    public async Task GetByHabitIdAsync_ShouldReturnSortedByDate()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new HabitEntryRepository(factory);

        await repo.CreateAsync(new HabitEntry { HabitId = 1, Date = new DateTime(2024, 6, 15) });
        await repo.CreateAsync(new HabitEntry { HabitId = 1, Date = new DateTime(2024, 6, 10) });
        await repo.CreateAsync(new HabitEntry { HabitId = 1, Date = new DateTime(2024, 6, 20) });
        await repo.CreateAsync(new HabitEntry { HabitId = 2, Date = new DateTime(2024, 6, 1) });

        var result = await repo.GetByHabitIdAsync(1);

        result.Should().HaveCount(3);
        result[0].Date.Should().Be(new DateTime(2024, 6, 10));
        result[2].Date.Should().Be(new DateTime(2024, 6, 20));
    }

    [Fact]
    public async Task GetByDateRangeAsync_ShouldFilterByUserAndDate()
    {
        var dbName = Guid.NewGuid().ToString();
        var factory = InMemoryDbHelper.CreateFactory(dbName);

        await using (var db = factory.CreateDbContext())
        {
            db.Habits.Add(new Habit { Id = 1, UserId = 1, Name = "H1", IsActive = true });
            db.Habits.Add(new Habit { Id = 2, UserId = 2, Name = "H2", IsActive = true });
            await db.SaveChangesAsync();
        }

        var repo = new HabitEntryRepository(factory);

        await repo.CreateAsync(new HabitEntry { HabitId = 1, Date = new DateTime(2024, 6, 5) });
        await repo.CreateAsync(new HabitEntry { HabitId = 1, Date = new DateTime(2024, 6, 15) });
        await repo.CreateAsync(new HabitEntry { HabitId = 1, Date = new DateTime(2024, 7, 1) });
        await repo.CreateAsync(new HabitEntry { HabitId = 2, Date = new DateTime(2024, 6, 10) });

        var result = await repo.GetByDateRangeAsync(
            1, new DateTime(2024, 6, 1), new DateTime(2024, 6, 30));

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByDateAsync_ShouldReturnEntriesForSpecificDate()
    {
        var dbName = Guid.NewGuid().ToString();
        var factory = InMemoryDbHelper.CreateFactory(dbName);

        await using (var db = factory.CreateDbContext())
        {
            db.Habits.Add(new Habit { Id = 1, UserId = 1, Name = "H", IsActive = true });
            await db.SaveChangesAsync();
        }

        var repo = new HabitEntryRepository(factory);
        var date = new DateTime(2024, 6, 15);

        await repo.CreateAsync(new HabitEntry { HabitId = 1, Date = date, ScheduledTime = new TimeSpan(9, 0, 0) });
        await repo.CreateAsync(new HabitEntry { HabitId = 1, Date = date, ScheduledTime = new TimeSpan(8, 0, 0) });
        await repo.CreateAsync(new HabitEntry { HabitId = 1, Date = date.AddDays(1) });

        var result = await repo.GetByDateAsync(1, date);

        result.Should().HaveCount(2);

        result[0].ScheduledTime.Should().Be(new TimeSpan(8, 0, 0));
        result[1].ScheduledTime.Should().Be(new TimeSpan(9, 0, 0));
    }

    [Fact]
    public async Task GetByHabitAndDateAsync_ShouldFindMatchingEntry()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new HabitEntryRepository(factory);

        var time = new TimeSpan(10, 0, 0);
        var date = new DateTime(2024, 6, 15);
        await repo.CreateAsync(new HabitEntry { HabitId = 1, Date = date, ScheduledTime = time });

        var found = await repo.GetByHabitAndDateAsync(1, date, time);

        found.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByHabitAndDateAsync_NoMatch_ShouldReturnNull()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new HabitEntryRepository(factory);

        var found = await repo.GetByHabitAndDateAsync(1, DateTime.Today, null);

        found.Should().BeNull();
    }

        [Fact]
    public async Task UpdateAsync_ShouldChangeEntry()
    {
        var dbName = Guid.NewGuid().ToString();
        var factory = InMemoryDbHelper.CreateFactory(dbName);

        await using (var db = factory.CreateDbContext())
        {
            db.Habits.Add(new Habit { Id = 1, UserId = 1, Name = "H", IsActive = true });
            await db.SaveChangesAsync();
        }

        var repo = new HabitEntryRepository(factory);

        var entry = await repo.CreateAsync(new HabitEntry
        {
            HabitId = 1,
            Date = DateTime.Today,
            IsCompleted = false
        });

        entry.IsCompleted = true;
        entry.CompletedAt = DateTime.UtcNow;
        await repo.UpdateAsync(entry);

        var found = await repo.GetByIdAsync(entry.Id);
        found!.IsCompleted.Should().BeTrue();
        found.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntry()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new HabitEntryRepository(factory);

        var entry = await repo.CreateAsync(new HabitEntry { HabitId = 1, Date = DateTime.Today });

        await repo.DeleteAsync(entry.Id);

        var found = await repo.GetByIdAsync(entry.Id);
        found.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistent_ShouldNotThrow()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new HabitEntryRepository(factory);

        var act = async () => await repo.DeleteAsync(999);

        await act.Should().NotThrowAsync();
    }
}