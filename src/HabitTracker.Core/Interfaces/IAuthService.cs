namespace HabitTracker.Core.Interfaces;

using HabitTracker.Core.Models;

public interface IAuthService
{
    Task<(bool Success, string Error, User? User)> RegisterAsync(string username, string password, string displayName);
    Task<(bool Success, string Error, User? User)> LoginAsync(string username, string password);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}