namespace HabitTracker.Core.Interfaces;

using HabitTracker.Core.Models;

public interface IHabitEntryRepository
{
    Task<HabitEntry?> GetByIdAsync(int id);
    Task<List<HabitEntry>> GetByHabitIdAsync(int habitId);
    Task<List<HabitEntry>> GetByDateRangeAsync(int userId, DateTime from, DateTime to);
    Task<List<HabitEntry>> GetByDateAsync(int userId, DateTime date);
    Task<HabitEntry> CreateAsync(HabitEntry entry);
    Task UpdateAsync(HabitEntry entry);
    Task DeleteAsync(int id);
    Task<HabitEntry?> GetByHabitAndDateAsync(int habitId, DateTime date, TimeSpan? scheduledTime);
}