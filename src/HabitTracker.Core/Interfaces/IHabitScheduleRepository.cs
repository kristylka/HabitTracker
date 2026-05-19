namespace HabitTracker.Core.Interfaces;

using HabitTracker.Core.Models;

public interface IHabitScheduleRepository
{
    Task<List<HabitSchedule>> GetByHabitIdAsync(int habitId);
    Task<List<HabitSchedule>> GetByUserAndDayAsync(int userId, DayOfWeek day);
    Task<HabitSchedule> CreateAsync(HabitSchedule schedule);
    Task UpdateAsync(HabitSchedule schedule);
    Task DeleteAsync(int id);
    Task DeleteByHabitIdAsync(int habitId);
}