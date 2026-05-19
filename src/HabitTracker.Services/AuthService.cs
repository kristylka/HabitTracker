namespace HabitTracker.Services;

using HabitTracker.Core.Interfaces;
using HabitTracker.Core.Models;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;

    public AuthService(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<(bool Success, string Error, User? User)> RegisterAsync(
        string username, string password, string displayName)
    {
        if (string.IsNullOrWhiteSpace(username))
            return (false, "Имя пользователя не может быть пустым", null);

        if (string.IsNullOrWhiteSpace(password))
            return (false, "Пароль не может быть пустым", null);

        if (password.Length < 4)
            return (false, "Пароль должен быть минимум 4 символа", null);

        if (string.IsNullOrWhiteSpace(displayName))
            return (false, "Отображаемое имя не может быть пустым", null);

        if (await _userRepo.ExistsAsync(username))
            return (false, "Пользователь с таким именем уже существует", null);

        var user = new User
        {
            Username = username.Trim(),
            PasswordHash = HashPassword(password),
            DisplayName = displayName.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.CreateAsync(user);
        return (true, string.Empty, user);
    }

    public async Task<(bool Success, string Error, User? User)> LoginAsync(
        string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            return (false, "Имя пользователя не может быть пустым", null);

        if (string.IsNullOrWhiteSpace(password))
            return (false, "Пароль не может быть пустым", null);

        var user = await _userRepo.GetByUsernameAsync(username.Trim());
        if (user == null)
            return (false, "Пользователь не найден", null);

        if (!VerifyPassword(password, user.PasswordHash))
            return (false, "Неверный пароль", null);

        return (true, string.Empty, user);
    }

    public string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    public bool VerifyPassword(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}