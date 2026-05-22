namespace HabitTracker.Core.Tests.Messages;

using FluentAssertions;
using HabitTracker.Core.Messages;
using HabitTracker.Core.Models;

public class MessagesTests
{
    [Fact]
    public void UserLoggedInMessage_ShouldStoreUser()
    {
        var user = new User { Id = 1, Username = "test" };
        var msg = new UserLoggedInMessage(user);

        msg.User.Should().Be(user);
        msg.User.Username.Should().Be("test");
    }

    [Fact]
    public void UserLoggedOutMessage_CanBeCreated()
    {
        var msg = new UserLoggedOutMessage();
        msg.Should().NotBeNull();
    }

    [Fact]
    public void HabitChangedMessage_CanBeCreated()
    {
        var msg = new HabitChangedMessage();
        msg.Should().NotBeNull();
    }

    [Fact]
    public void NavigationMessage_ShouldStoreViewName()
    {
        var msg = new NavigationMessage("Calendar");

        msg.ViewName.Should().Be("Calendar");
        msg.Parameter.Should().BeNull();
    }

    [Fact]
    public void NavigationMessage_ShouldStoreParameter()
    {
        var param = new { Id = 5 };
        var msg = new NavigationMessage("Profile", param);

        msg.ViewName.Should().Be("Profile");
        msg.Parameter.Should().Be(param);
    }
}