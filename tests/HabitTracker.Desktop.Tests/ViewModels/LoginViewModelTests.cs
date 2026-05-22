namespace HabitTracker.Desktop.Tests.ViewModels;

using CommunityToolkit.Mvvm.Messaging;
using FluentAssertions;
using HabitTracker.Core.Interfaces;
using HabitTracker.Core.Messages;
using HabitTracker.Core.Models;
using HabitTracker.Desktop.ViewModels;
using Moq;

public class LoginViewModelTests
{
    private readonly Mock<IAuthService> _authMock;
    private readonly IMessenger _messenger;
    private readonly LoginViewModel _vm;

    public LoginViewModelTests()
    {
        _authMock = new Mock<IAuthService>();
        _messenger = new WeakReferenceMessenger();
        _vm = new LoginViewModel(_authMock.Object, _messenger);
    }

    [Fact]
    public void NewViewModel_ShouldHaveEmptyFields()
    {
        _vm.Username.Should().BeEmpty();
        _vm.Password.Should().BeEmpty();
        _vm.ErrorMessage.Should().BeEmpty();
        _vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoginCommand_SuccessfulLogin_SendsUserLoggedInMessage()
    {
        var user = new User { Id = 1, Username = "user" };
        _authMock.Setup(a => a.LoginAsync("user", "password"))
            .ReturnsAsync((true, string.Empty, user));

        User? received = null;
        _messenger.Register<LoginViewModelTests, UserLoggedInMessage>(this, (r, m) => received = m.User);

        _vm.Username = "user";
        _vm.Password = "password";

        await _vm.LoginCommand.ExecuteAsync(null);

        received.Should().NotBeNull();
        received!.Id.Should().Be(1);
        _vm.ErrorMessage.Should().BeEmpty();
        _vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoginCommand_FailedLogin_SetsErrorMessage()
    {
        _authMock.Setup(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((false, "Неверный пароль", null));

        _vm.Username = "user";
        _vm.Password = "wrong";

        await _vm.LoginCommand.ExecuteAsync(null);

        _vm.ErrorMessage.Should().Be("Неверный пароль");
        _vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoginCommand_Exception_SetsErrorMessage()
    {
        _authMock.Setup(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("DB error"));

        _vm.Username = "user";
        _vm.Password = "pwd";

        await _vm.LoginCommand.ExecuteAsync(null);

        _vm.ErrorMessage.Should().Contain("DB error");
        _vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void GoToRegisterCommand_SendsNavigationMessage()
    {
        string? viewName = null;
        _messenger.Register<LoginViewModelTests, NavigationMessage>(this, (r, m) => viewName = m.ViewName);

        _vm.GoToRegisterCommand.Execute(null);

        viewName.Should().Be("Register");
    }
}