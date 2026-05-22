namespace HabitTracker.Core.Tests.Models;

using FluentAssertions;
using HabitTracker.Core.Models;

public class UserTests
{
    [Fact]
    public void NewUser_ShouldHaveDefaultValues()
    {
        var user = new User();

        user.Id.Should().Be(0);
        user.Username.Should().BeEmpty();
        user.PasswordHash.Should().BeEmpty();
        user.DisplayName.Should().BeEmpty();
        user.Habits.Should().NotBeNull().And.BeEmpty();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void User_ShouldSetPropertiesCorrectly()
    {
        var created = new DateTime(2024, 1, 1);
        var user = new User
        {
            Id = 5,
            Username = "test_user",
            PasswordHash = "hash",
            DisplayName = "Test User",
            CreatedAt = created
        };

        user.Id.Should().Be(5);
        user.Username.Should().Be("test_user");
        user.PasswordHash.Should().Be("hash");
        user.DisplayName.Should().Be("Test User");
        user.CreatedAt.Should().Be(created);
    }

    [Fact]
    public void User_CanAddHabits()
    {
        var user = new User();
        var habit = new Habit { Name = "Test" };

        user.Habits.Add(habit);

        user.Habits.Should().HaveCount(1);
        user.Habits[0].Name.Should().Be("Test");
    }
}