namespace HabitTracker.Core.Interfaces;

using HabitTracker.Core.Models;

public interface IHabitRepository
{
    Task<Habit?> GetByIdAsync(int id);
    Task<List<Habit>> GetByUserIdAsync(int userId);
    Task<Habit> CreateAsync(Habit habit);
    Task UpdateAsync(Habit habit);
    Task DeleteAsync(int id);
}