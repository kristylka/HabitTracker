namespace HabitTracker.Desktop.Tests.ViewModels;

using CommunityToolkit.Mvvm.Messaging;
using FluentAssertions;
using HabitTracker.Core.Interfaces;
using HabitTracker.Core.Messages;
using HabitTracker.Core.Models;
using HabitTracker.Desktop.ViewModels;
using Moq;

public class RegisterViewModelTests
{
    private readonly Mock<IAuthService> _authMock;
    private readonly IMessenger _messenger;
    private readonly RegisterViewModel _vm;

    public RegisterViewModelTests()
    {
        _authMock = new Mock<IAuthService>();
        _messenger = new WeakReferenceMessenger();
        _vm = new RegisterViewModel(_authMock.Object, _messenger);
    }

    [Fact]
    public void NewViewModel_ShouldHaveEmptyFields()
    {
        _vm.Username.Should().BeEmpty();
        _vm.Password.Should().BeEmpty();
        _vm.ConfirmPassword.Should().BeEmpty();
        _vm.DisplayName.Should().BeEmpty();
        _vm.ErrorMessage.Should().BeEmpty();
        _vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterCommand_PasswordsDoNotMatch_SetsError()
    {
        _vm.Username = "user";
        _vm.Password = "password";
        _vm.ConfirmPassword = "different";
        _vm.DisplayName = "User";

        await _vm.RegisterCommand.ExecuteAsync(null);

        _vm.ErrorMessage.Should().Contain("не совпадают");
        _authMock.Verify(a => a.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterCommand_SuccessfulRegister_SendsUserLoggedInMessage()
    {
        var user = new User { Id = 1, Username = "user" };
        _authMock.Setup(a => a.RegisterAsync("user", "password", "User"))
            .ReturnsAsync((true, string.Empty, user));

        User? received = null;
        _messenger.Register<RegisterViewModelTests, UserLoggedInMessage>(this, (r, m) => received = m.User);

        _vm.Username = "user";
        _vm.Password = "password";
        _vm.ConfirmPassword = "password";
        _vm.DisplayName = "User";

        await _vm.RegisterCommand.ExecuteAsync(null);

        received.Should().NotBeNull();
        received!.Id.Should().Be(1);
        _vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterCommand_FailedRegister_SetsErrorMessage()
    {
        _authMock.Setup(a => a.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((false, "уже существует", null));

        _vm.Username = "user";
        _vm.Password = "password";
        _vm.ConfirmPassword = "password";
        _vm.DisplayName = "User";

        await _vm.RegisterCommand.ExecuteAsync(null);

        _vm.ErrorMessage.Should().Be("уже существует");
    }

    [Fact]
    public async Task RegisterCommand_Exception_SetsErrorMessage()
    {
        _authMock.Setup(a => a.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("DB error"));

        _vm.Username = "user";
        _vm.Password = "pwd";
        _vm.ConfirmPassword = "pwd";
        _vm.DisplayName = "Name";

        await _vm.RegisterCommand.ExecuteAsync(null);

        _vm.ErrorMessage.Should().Contain("DB error");
    }

    [Fact]
    public void GoToLoginCommand_SendsNavigationMessage()
    {
        string? viewName = null;
        _messenger.Register<RegisterViewModelTests, NavigationMessage>(this, (r, m) => viewName = m.ViewName);

        _vm.GoToLoginCommand.Execute(null);

        viewName.Should().Be("Login");
    }
}