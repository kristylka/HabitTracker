namespace HabitTracker.Data.Tests.Repositories;

using FluentAssertions;
using HabitTracker.Core.Models;
using HabitTracker.Data.Repositories;
using HabitTracker.Data.Tests.Helpers;

public class UserRepositoryTests
{
    [Fact]
    public async Task CreateAsync_ShouldAddUser()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new UserRepository(factory);

        var user = new User
        {
            Username = "test",
            PasswordHash = "hash",
            DisplayName = "Test User"
        };

        var created = await repo.CreateAsync(user);

        created.Id.Should().BeGreaterThan(0);
        created.Username.Should().Be("test");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ShouldReturnUser()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new UserRepository(factory);

        var user = await repo.CreateAsync(new User
        {
            Username = "test",
            PasswordHash = "h",
            DisplayName = "T"
        });

        var found = await repo.GetByIdAsync(user.Id);

        found.Should().NotBeNull();
        found!.Username.Should().Be("test");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ShouldReturnNull()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new UserRepository(factory);

        var found = await repo.GetByIdAsync(999);

        found.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_ExistingUser_ShouldReturnUser()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new UserRepository(factory);

        await repo.CreateAsync(new User
        {
            Username = "alice",
            PasswordHash = "h",
            DisplayName = "Alice"
        });

        var found = await repo.GetByUsernameAsync("alice");

        found.Should().NotBeNull();
        found!.DisplayName.Should().Be("Alice");
    }

    [Fact]
    public async Task GetByUsernameAsync_NonExistent_ShouldReturnNull()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new UserRepository(factory);

        var found = await repo.GetByUsernameAsync("ghost");

        found.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_ExistingUser_ShouldReturnTrue()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new UserRepository(factory);

        await repo.CreateAsync(new User
        {
            Username = "bob",
            PasswordHash = "h",
            DisplayName = "Bob"
        });

        var exists = await repo.ExistsAsync("bob");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistent_ShouldReturnFalse()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new UserRepository(factory);

        var exists = await repo.ExistsAsync("nobody");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldChangeUser()
    {
        var factory = InMemoryDbHelper.CreateFactory();
        var repo = new UserRepository(factory);

        var user = await repo.CreateAsync(new User
        {
            Username = "user",
            PasswordHash = "h",
            DisplayName = "Old Name"
        });

        user.DisplayName = "New Name";
        await repo.UpdateAsync(user);

        var found = await repo.GetByIdAsync(user.Id);
        found!.DisplayName.Should().Be("New Name");
    }
}