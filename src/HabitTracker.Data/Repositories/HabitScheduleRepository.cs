namespace HabitTracker.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using HabitTracker.Core.Interfaces;
using HabitTracker.Core.Models;

public class HabitScheduleRepository : IHabitScheduleRepository
{
    private readonly AppDbContext _db;

    public HabitScheduleRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<HabitSchedule>> GetByHabitIdAsync(int habitId)
        => await _db.HabitSchedules
            .Where(s => s.HabitId == habitId)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.Time)
            .ToListAsync();

    public async Task<List<HabitSchedule>> GetByUserAndDayAsync(int userId, DayOfWeek day)
        => await _db.HabitSchedules
            .Include(s => s.Habit)
            .Where(s => s.Habit!.UserId == userId && s.Habit.IsActive && s.DayOfWeek == day)
            .OrderBy(s => s.Time)
            .ToListAsync();

    public async Task<HabitSchedule> CreateAsync(HabitSchedule schedule)
    {
        _db.HabitSchedules.Add(schedule);
        await _db.SaveChangesAsync();
        return schedule;
    }

    public async Task UpdateAsync(HabitSchedule schedule)
    {
        _db.HabitSchedules.Update(schedule);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var schedule = await _db.HabitSchedules.FindAsync(id);
        if (schedule != null)
        {
            _db.HabitSchedules.Remove(schedule);
            await _db.SaveChangesAsync();
        }
    }

    public async Task DeleteByHabitIdAsync(int habitId)
    {
        var schedules = await _db.HabitSchedules
            .Where(s => s.HabitId == habitId)
            .ToListAsync();
        _db.HabitSchedules.RemoveRange(schedules);
        await _db.SaveChangesAsync();
    }
}