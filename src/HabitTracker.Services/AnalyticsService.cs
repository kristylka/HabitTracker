namespace HabitTracker.Services;

using HabitTracker.Core.Interfaces;
using HabitTracker.Core.Models;

public class AnalyticsService : IAnalyticsService
{
    private readonly IHabitEntryRepository _entryRepo;
    private readonly IHabitRepository _habitRepo;

    public AnalyticsService(IHabitEntryRepository entryRepo, IHabitRepository habitRepo)
    {
        _entryRepo = entryRepo;
        _habitRepo = habitRepo;
    }

    public async Task<double> GetCompletionRateAsync(int userId, DateTime from, DateTime to)
    {
        var entries = await _entryRepo.GetByDateRangeAsync(userId, from, to);
        if (entries.Count == 0) return 0;

        return (double)entries.Count(e => e.IsCompleted) / entries.Count * 100;
    }

    public async Task<int> GetCurrentStreakAsync(int habitId)
    {
        var entries = await _entryRepo.GetByHabitIdAsync(habitId);
        var ordered = entries
            .Where(e => e.Date.Date <= DateTime.Today)
            .OrderByDescending(e => e.Date)
            .ToList();

        int streak = 0;
        foreach (var entry in ordered)
        {
            if (entry.IsCompleted)
                streak++;
            else
                break;
        }

        return streak;
    }

    public async Task<int> GetBestStreakAsync(int habitId)
    {
        var entries = await _entryRepo.GetByHabitIdAsync(habitId);
        var ordered = entries.OrderBy(e => e.Date).ToList();

        int best = 0;
        int current = 0;

        foreach (var entry in ordered)
        {
            if (entry.IsCompleted)
            {
                current++;
                if (current > best) best = current;
            }
            else
            {
                current = 0;
            }
        }

        return best;
    }

    public async Task<Dictionary<string, double>> GetHabitCompletionRatesAsync(
        int userId, DateTime from, DateTime to)
    {
        var entries = await _entryRepo.GetByDateRangeAsync(userId, from, to);
        var result = new Dictionary<string, double>();

        var grouped = entries.GroupBy(e => e.Habit?.Name ?? "Unknown");
        foreach (var group in grouped)
        {
            var total = group.Count();
            var completed = group.Count(e => e.IsCompleted);
            result[group.Key] = total > 0 ? (double)completed / total * 100 : 0;
        }

        return result;
    }

    public async Task<Dictionary<DateTime, int>> GetDailyCompletionCountsAsync(
        int userId, DateTime from, DateTime to)
    {
        var entries = await _entryRepo.GetByDateRangeAsync(userId, from, to);

        return entries
            .Where(e => e.IsCompleted)
            .GroupBy(e => e.Date.Date)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}