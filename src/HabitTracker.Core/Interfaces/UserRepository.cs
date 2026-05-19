namespace HabitTracker.Core.Interfaces;

using HabitTracker.Core.Models;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> ExistsAsync(string username);
}