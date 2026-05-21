namespace HabitTracker.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using HabitTracker.Core.Interfaces;
using HabitTracker.Core.Models;

public class HabitScheduleRepository : IHabitScheduleRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public HabitScheduleRepository(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<List<HabitSchedule>> GetByHabitIdAsync(int habitId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var list = await db.HabitSchedules
            .Where(s => s.HabitId == habitId)
            .ToListAsync();
        return list.OrderBy(s => s.DayOfWeek).ThenBy(s => s.Time).ToList();
    }

    public async Task<List<HabitSchedule>> GetByUserAndDayAsync(int userId, DayOfWeek day)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var list = await db.HabitSchedules
            .Include(s => s.Habit)
            .Where(s => s.Habit!.UserId == userId && s.Habit.IsActive && s.DayOfWeek == day)
            .ToListAsync();
        return list.OrderBy(s => s.Time).ToList();
    }

    public async Task<HabitSchedule> CreateAsync(HabitSchedule schedule)
    {
        await using var db = await _factory.CreateDbContextAsync();
        db.HabitSchedules.Add(schedule);
        await db.SaveChangesAsync();
        return schedule;
    }

    public async Task UpdateAsync(HabitSchedule schedule)
    {
        await using var db = await _factory.CreateDbContextAsync();
        db.HabitSchedules.Update(schedule);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var schedule = await db.HabitSchedules.FindAsync(id);
        if (schedule != null)
        {
            db.HabitSchedules.Remove(schedule);
            await db.SaveChangesAsync();
        }
    }

    public async Task DeleteByHabitIdAsync(int habitId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var schedules = await db.HabitSchedules
            .Where(s => s.HabitId == habitId)
            .ToListAsync();
        db.HabitSchedules.RemoveRange(schedules);
        await db.SaveChangesAsync();
    }
}