namespace HabitTracker.Services.Tests.Services;

using FluentAssertions;
using HabitTracker.Core.Interfaces;
using HabitTracker.Core.Models;
using HabitTracker.Services;
using Moq;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _authService = new AuthService(_userRepoMock.Object);
    }

    [Fact]
    public void HashPassword_ShouldReturnNonEmptyHash()
    {
        var hash = _authService.HashPassword("password123");

        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe("password123");
    }

    [Fact]
    public void HashPassword_SameInput_ShouldReturnDifferentHashes()
    {
        var hash1 = _authService.HashPassword("password");
        var hash2 = _authService.HashPassword("password");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ShouldReturnTrue()
    {
        var hash = _authService.HashPassword("MySecret123");

        var result = _authService.VerifyPassword("MySecret123", hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ShouldReturnFalse()
    {
        var hash = _authService.HashPassword("MySecret123");

        var result = _authService.VerifyPassword("WrongPassword", hash);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterAsync_ValidData_ShouldCreateUser()
    {
        _userRepoMock.Setup(r => r.ExistsAsync("newuser")).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.Id = 1; return u; });

        var (success, error, user) = await _authService.RegisterAsync(
            "newuser", "password", "New User");

        success.Should().BeTrue();
        error.Should().BeEmpty();
        user.Should().NotBeNull();
        user!.Username.Should().Be("newuser");
        user.DisplayName.Should().Be("New User");
        user.PasswordHash.Should().NotBeNullOrEmpty();
        user.PasswordHash.Should().NotBe("password");

        _userRepoMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_EmptyUsername_ShouldFail()
    {
        var (success, error, user) = await _authService.RegisterAsync(
            "", "password", "Name");

        success.Should().BeFalse();
        error.Should().Contain("Имя пользователя");
        user.Should().BeNull();
        _userRepoMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhitespaceUsername_ShouldFail()
    {
        var (success, error, user) = await _authService.RegisterAsync(
            "   ", "password", "Name");

        success.Should().BeFalse();
        error.Should().NotBeEmpty();
        user.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_EmptyPassword_ShouldFail()
    {
        var (success, error, user) = await _authService.RegisterAsync(
            "user", "", "Name");

        success.Should().BeFalse();
        error.Should().Contain("Пароль");
        user.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_ShortPassword_ShouldFail()
    {
        var (success, error, user) = await _authService.RegisterAsync(
            "user", "12", "Name");

        success.Should().BeFalse();
        error.Should().Contain("4 символа");
        user.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_EmptyDisplayName_ShouldFail()
    {
        var (success, error, user) = await _authService.RegisterAsync(
            "user", "password", "");

        success.Should().BeFalse();
        error.Should().Contain("Отображаемое имя");
        user.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_ExistingUsername_ShouldFail()
    {
        _userRepoMock.Setup(r => r.ExistsAsync("existing")).ReturnsAsync(true);

        var (success, error, user) = await _authService.RegisterAsync(
            "existing", "password", "Name");

        success.Should().BeFalse();
        error.Should().Contain("уже существует");
        user.Should().BeNull();
        _userRepoMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldTrimWhitespace()
    {
        _userRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        var (success, _, user) = await _authService.RegisterAsync(
            "  user  ", "password", "  Name  ");

        success.Should().BeTrue();
        user!.Username.Should().Be("user");
        user.DisplayName.Should().Be("Name");
    }

    [Fact]
    public async Task RegisterAsync_ShouldSetCreatedAt()
    {
        _userRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        var before = DateTime.UtcNow;
        var (_, _, user) = await _authService.RegisterAsync(
            "user", "password", "Name");
        var after = DateTime.UtcNow;

        user!.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ShouldReturnUser()
    {
        var hash = _authService.HashPassword("password");
        var existing = new User
        {
            Id = 1,
            Username = "user",
            PasswordHash = hash,
            DisplayName = "User"
        };
        _userRepoMock.Setup(r => r.GetByUsernameAsync("user")).ReturnsAsync(existing);

        var (success, error, user) = await _authService.LoginAsync("user", "password");

        success.Should().BeTrue();
        error.Should().BeEmpty();
        user.Should().Be(existing);
    }

    [Fact]
    public async Task LoginAsync_EmptyUsername_ShouldFail()
    {
        var (success, error, user) = await _authService.LoginAsync("", "password");

        success.Should().BeFalse();
        error.Should().Contain("Имя пользователя");
        user.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_EmptyPassword_ShouldFail()
    {
        var (success, error, user) = await _authService.LoginAsync("user", "");

        success.Should().BeFalse();
        error.Should().Contain("Пароль");
        user.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_NonExistentUser_ShouldFail()
    {
        _userRepoMock.Setup(r => r.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var (success, error, user) = await _authService.LoginAsync("ghost", "password");

        success.Should().BeFalse();
        error.Should().Contain("не найден");
        user.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ShouldFail()
    {
        var hash = _authService.HashPassword("correctpassword");
        var existing = new User { Username = "user", PasswordHash = hash };
        _userRepoMock.Setup(r => r.GetByUsernameAsync("user")).ReturnsAsync(existing);

        var (success, error, user) = await _authService.LoginAsync("user", "wrongpassword");

        success.Should().BeFalse();
        error.Should().Contain("Неверный пароль");
        user.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldTrimUsername()
    {
        var hash = _authService.HashPassword("password");
        var existing = new User { Username = "user", PasswordHash = hash };
        _userRepoMock.Setup(r => r.GetByUsernameAsync("user")).ReturnsAsync(existing);

        var (success, _, _) = await _authService.LoginAsync("  user  ", "password");

        success.Should().BeTrue();
        _userRepoMock.Verify(r => r.GetByUsernameAsync("user"), Times.Once);
    }
}