namespace HabitTracker.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using HabitTracker.Core.Interfaces;
using HabitTracker.Core.Models;

public class HabitEntryRepository : IHabitEntryRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public HabitEntryRepository(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<HabitEntry?> GetByIdAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.HabitEntries.Include(e => e.Habit).FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<List<HabitEntry>> GetByHabitIdAsync(int habitId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var list = await db.HabitEntries
            .Where(e => e.HabitId == habitId)
            .ToListAsync();
        return list.OrderBy(e => e.Date).ToList();
    }

    public async Task<List<HabitEntry>> GetByDateRangeAsync(int userId, DateTime from, DateTime to)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var list = await db.HabitEntries
            .Include(e => e.Habit)
            .Where(e => e.Habit!.UserId == userId && e.Date >= from && e.Date <= to)
            .ToListAsync();
        return list.OrderBy(e => e.Date).ThenBy(e => e.ScheduledTime).ToList();
    }

    public async Task<List<HabitEntry>> GetByDateAsync(int userId, DateTime date)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var list = await db.HabitEntries
            .Include(e => e.Habit)
            .Where(e => e.Habit!.UserId == userId && e.Date.Date == date.Date)
            .ToListAsync();
        return list.OrderBy(e => e.ScheduledTime).ToList();
    }

    public async Task<HabitEntry> CreateAsync(HabitEntry entry)
    {
        await using var db = await _factory.CreateDbContextAsync();
        db.HabitEntries.Add(entry);
        await db.SaveChangesAsync();
        return entry;
    }

    public async Task UpdateAsync(HabitEntry entry)
    {
        await using var db = await _factory.CreateDbContextAsync();
        db.HabitEntries.Update(entry);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var entry = await db.HabitEntries.FindAsync(id);
        if (entry != null)
        {
            db.HabitEntries.Remove(entry);
            await db.SaveChangesAsync();
        }
    }

    public async Task<HabitEntry?> GetByHabitAndDateAsync(int habitId, DateTime date, TimeSpan? scheduledTime)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.HabitEntries
            .FirstOrDefaultAsync(e => e.HabitId == habitId
                && e.Date.Date == date.Date
                && e.ScheduledTime == scheduledTime);
    }
}