namespace HabitTracker.App.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTracker.Core.Interfaces;

public partial class AnalyticsViewModel : ViewModelBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IHabitService _habitService;
    private int _userId;

    [ObservableProperty]
    private double _overallCompletionRate;

    [ObservableProperty]
    private ObservableCollection<HabitStatItem> _habitStats = new();

    [ObservableProperty]
    private ObservableCollection<DailyStatItem> _dailyStats = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _selectedPeriodDays = 7;

    public AnalyticsViewModel(IAnalyticsService analyticsService, IHabitService habitService)
    {
        _analyticsService = analyticsService;
        _habitService = habitService;
    }

    public void SetUser(int userId)
    {
        _userId = userId;
    }

    [RelayCommand]
    private async Task LoadAnalyticsAsync()
    {
        if (_userId == 0) return;

        IsLoading = true;
        try
        {
            var to = DateTime.Today;
            var from = to.AddDays(-SelectedPeriodDays + 1);

            OverallCompletionRate = await _analyticsService.GetCompletionRateAsync(_userId, from, to);

            var habitRates = await _analyticsService.GetHabitCompletionRatesAsync(_userId, from, to);
            var stats = new ObservableCollection<HabitStatItem>();
            var habits = await _habitService.GetUserHabitsAsync(_userId);

            foreach (var habit in habits)
            {
                var rate = habitRates.ContainsKey(habit.Name) ? habitRates[habit.Name] : 0;
                var currentStreak = await _analyticsService.GetCurrentStreakAsync(habit.Id);
                var bestStreak = await _analyticsService.GetBestStreakAsync(habit.Id);

                stats.Add(new HabitStatItem
                {
                    Name = habit.Name,
                    Color = habit.Color,
                    CompletionRate = rate,
                    CurrentStreak = currentStreak,
                    BestStreak = bestStreak
                });
            }
            HabitStats = stats;

            var dailyCounts = await _analyticsService.GetDailyCompletionCountsAsync(_userId, from, to);
            var dailyStatsList = new ObservableCollection<DailyStatItem>();
            for (var d = from; d <= to; d = d.AddDays(1))
            {
                dailyStatsList.Add(new DailyStatItem
                {
                    Date = d,
                    Count = dailyCounts.ContainsKey(d) ? dailyCounts[d] : 0
                });
            }
            DailyStats = dailyStatsList;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SetPeriodAsync(int days)
    {
        SelectedPeriodDays = days;
        await LoadAnalyticsAsync();
    }
}

public class HabitStatItem
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#4CAF50";
    public double CompletionRate { get; set; }
    public int CurrentStreak { get; set; }
    public int BestStreak { get; set; }
}

public class DailyStatItem
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}