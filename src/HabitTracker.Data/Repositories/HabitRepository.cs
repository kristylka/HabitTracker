namespace HabitTracker.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using HabitTracker.Core.Interfaces;
using HabitTracker.Core.Models;

public class HabitRepository : IHabitRepository
{
    private readonly AppDbContext _db;

    public HabitRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Habit?> GetByIdAsync(int id)
        => await _db.Habits
            .Include(h => h.Schedules)
            .Include(h => h.Entries)
            .FirstOrDefaultAsync(h => h.Id == id);

    public async Task<List<Habit>> GetByUserIdAsync(int userId)
        => await _db.Habits
            .Include(h => h.Schedules)
            .Where(h => h.UserId == userId && h.IsActive)
            .ToListAsync();

    public async Task<Habit> CreateAsync(Habit habit)
    {
        _db.Habits.Add(habit);
        await _db.SaveChangesAsync();
        return habit;
    }

    public async Task UpdateAsync(Habit habit)
    {
        _db.Habits.Update(habit);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var habit = await _db.Habits.FindAsync(id);
        if (habit != null)
        {
            _db.Habits.Remove(habit);
            await _db.SaveChangesAsync();
        }
    }
}