namespace HabitTracker.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using HabitTracker.Core.Interfaces;
using HabitTracker.Core.Models;

public class HabitRepository : IHabitRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public HabitRepository(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<Habit?> GetByIdAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Habits
            .Include(h => h.Schedules)
            .Include(h => h.Entries)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<List<Habit>> GetByUserIdAsync(int userId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Habits
            .Include(h => h.Schedules)
            .Where(h => h.UserId == userId && h.IsActive)
            .ToListAsync();
    }

    public async Task<Habit> CreateAsync(Habit habit)
    {
        await using var db = await _factory.CreateDbContextAsync();
        db.Habits.Add(habit);
        await db.SaveChangesAsync();
        return habit;
    }

    public async Task UpdateAsync(Habit habit)
    {
        await using var db = await _factory.CreateDbContextAsync();
        db.Habits.Update(habit);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var habit = await db.Habits.FindAsync(id);
        if (habit != null)
        {
            db.Habits.Remove(habit);
            await db.SaveChangesAsync();
        }
    }
}