namespace HabitTracker.Core.Interfaces;

using HabitTracker.Core.Models;

public interface IAnalyticsService
{
    Task<double> GetCompletionRateAsync(int userId, DateTime from, DateTime to);
    Task<int> GetCurrentStreakAsync(int habitId);
    Task<int> GetBestStreakAsync(int habitId);
    Task<Dictionary<string, double>> GetHabitCompletionRatesAsync(int userId, DateTime from, DateTime to);
    Task<Dictionary<DateTime, int>> GetDailyCompletionCountsAsync(int userId, DateTime from, DateTime to);
}