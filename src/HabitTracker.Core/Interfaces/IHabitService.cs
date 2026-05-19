namespace HabitTracker.Core.Interfaces;

using HabitTracker.Core.Models;

public interface IHabitService
{
    Task<List<Habit>> GetUserHabitsAsync(int userId);
    Task<Habit> CreateHabitAsync(int userId, string name, string description, string color,
        List<(DayOfWeek Day, TimeSpan Time, bool Notify)> schedules);
    Task UpdateHabitAsync(Habit habit);
    Task DeleteHabitAsync(int habitId);
    Task GenerateEntriesForDateAsync(int userId, DateTime date);
    Task<List<HabitEntry>> GetEntriesForDateAsync(int userId, DateTime date);
    Task<List<HabitEntry>> GetEntriesForRangeAsync(int userId, DateTime from, DateTime to);
    Task ToggleEntryCompletionAsync(int entryId);
}