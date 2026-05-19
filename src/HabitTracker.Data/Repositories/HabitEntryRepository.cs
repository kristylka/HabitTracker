namespace HabitTracker.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using HabitTracker.Core.Interfaces;
using HabitTracker.Core.Models;

public class HabitEntryRepository : IHabitEntryRepository
{
    private readonly AppDbContext _db;

    public HabitEntryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<HabitEntry?> GetByIdAsync(int id)
        => await _db.HabitEntries.Include(e => e.Habit).FirstOrDefaultAsync(e => e.Id == id);

    public async Task<List<HabitEntry>> GetByHabitIdAsync(int habitId)
        => await _db.HabitEntries
            .Where(e => e.HabitId == habitId)
            .OrderBy(e => e.Date)
            .ToListAsync();

    public async Task<List<HabitEntry>> GetByDateRangeAsync(int userId, DateTime from, DateTime to)
        => await _db.HabitEntries
            .Include(e => e.Habit)
            .Where(e => e.Habit!.UserId == userId && e.Date >= from && e.Date <= to)
            .OrderBy(e => e.Date)
            .ThenBy(e => e.ScheduledTime)
            .ToListAsync();

    public async Task<List<HabitEntry>> GetByDateAsync(int userId, DateTime date)
        => await _db.HabitEntries
            .Include(e => e.Habit)
            .Where(e => e.Habit!.UserId == userId && e.Date.Date == date.Date)
            .OrderBy(e => e.ScheduledTime)
            .ToListAsync();

    public async Task<HabitEntry> CreateAsync(HabitEntry entry)
    {
        _db.HabitEntries.Add(entry);
        await _db.SaveChangesAsync();
        return entry;
    }

    public async Task UpdateAsync(HabitEntry entry)
    {
        _db.HabitEntries.Update(entry);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entry = await _db.HabitEntries.FindAsync(id);
        if (entry != null)
        {
            _db.HabitEntries.Remove(entry);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<HabitEntry?> GetByHabitAndDateAsync(int habitId, DateTime date, TimeSpan? scheduledTime)
        => await _db.HabitEntries
            .FirstOrDefaultAsync(e => e.HabitId == habitId
                && e.Date.Date == date.Date
                && e.ScheduledTime == scheduledTime);
}