namespace HabitTracker.Desktop.Tests.ViewModels;

using CommunityToolkit.Mvvm.Messaging;
using FluentAssertions;
using HabitTracker.Core.Interfaces;
using HabitTracker.Core.Messages;
using HabitTracker.Core.Models;
using HabitTracker.Desktop.ViewModels;
using Moq;

public class ProfileViewModelTests
{
    private readonly IMessenger _messenger;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly ProfileViewModel _vm;

    public ProfileViewModelTests()
    {
        _messenger = new WeakReferenceMessenger();
        _notificationMock = new Mock<INotificationService>();
        _vm = new ProfileViewModel(_messenger, _notificationMock.Object);
    }

    [Fact]
    public void NewViewModel_ShouldHaveEmptyFields()
    {
        _vm.DisplayName.Should().BeEmpty();
        _vm.Username.Should().BeEmpty();
    }

    [Fact]
    public void SetUser_ShouldPopulateFields()
    {
        var user = new User
        {
            Id = 1,
            Username = "test",
            DisplayName = "Test User",
            CreatedAt = new DateTime(2024, 1, 1)
        };

        _vm.SetUser(user);

        _vm.Username.Should().Be("test");
        _vm.DisplayName.Should().Be("Test User");
        _vm.MemberSince.Should().Be(new DateTime(2024, 1, 1));
    }

    [Fact]
    public void LogoutCommand_SendsUserLoggedOutMessage()
    {
        var received = false;
        _messenger.Register<ProfileViewModelTests, UserLoggedOutMessage>(this, (r, m) => received = true);

        _vm.LogoutCommand.Execute(null);

        received.Should().BeTrue();
    }

    [Fact]
    public void TestNotificationCommand_ShouldCallNotificationService()
    {
        _vm.TestNotificationCommand.Execute(null);

        _notificationMock.Verify(n => n.ShowNotification(
            It.IsAny<string>(),
            It.IsAny<string>()),
            Times.Once);
    }
}