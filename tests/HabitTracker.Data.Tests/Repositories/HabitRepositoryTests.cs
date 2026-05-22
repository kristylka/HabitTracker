namespace HabitTracker.Data.Tests.Repositories;

using FluentAssertions;
using HabitTracker.Core.Models;
using HabitTracker.Data.Repositories;
using HabitTracker.Data.Tests.Helpers;

public class HabitRepositoryTests
{
    [Fact]
    public async Task CreateAsync_ShouldAddHabit()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new HabitRepository(factory);

        var habit = new Habit { Name = "Water", UserId = 1, IsActive = true };
        var created = await repo.CreateAsync(habit);

        created.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingHabit_ShouldReturnHabit()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new HabitRepository(factory);

        var habit = await repo.CreateAsync(new Habit
        {
            Name = "Read",
            UserId = 1,
            IsActive = true
        });

        var found = await repo.GetByIdAsync(habit.Id);

        found.Should().NotBeNull();
        found!.Name.Should().Be("Read");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ShouldReturnNull()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new HabitRepository(factory);

        var found = await repo.GetByIdAsync(999);

        found.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnOnlyActiveUserHabits()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new HabitRepository(factory);

        await repo.CreateAsync(new Habit { Name = "A", UserId = 1, IsActive = true });
        await repo.CreateAsync(new Habit { Name = "B", UserId = 1, IsActive = true });
        await repo.CreateAsync(new Habit { Name = "C", UserId = 1, IsActive = false });
        await repo.CreateAsync(new Habit { Name = "D", UserId = 2, IsActive = true });

        var result = await repo.GetByUserIdAsync(1);

        result.Should().HaveCount(2);
        result.Select(h => h.Name).Should().Contain(new[] { "A", "B" });
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateHabit()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new HabitRepository(factory);

        var habit = await repo.CreateAsync(new Habit { Name = "Old", UserId = 1, IsActive = true });

        habit.Name = "New";
        await repo.UpdateAsync(habit);

        var found = await repo.GetByIdAsync(habit.Id);
        found!.Name.Should().Be("New");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveHabit()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new HabitRepository(factory);

        var habit = await repo.CreateAsync(new Habit { Name = "X", UserId = 1, IsActive = true });

        await repo.DeleteAsync(habit.Id);

        var found = await repo.GetByIdAsync(habit.Id);
        found.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistent_ShouldNotThrow()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new HabitRepository(factory);

        var act = async () => await repo.DeleteAsync(999);

        await act.Should().NotThrowAsync();
    }
}