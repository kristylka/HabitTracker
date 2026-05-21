using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using HabitTracker.Core.Interfaces;

namespace HabitTracker.Desktop.Services;

public class NotificationScheduler : INotificationScheduler
{
    private readonly IHabitService _habitService;
    private readonly INotificationService _notificationService;
    private DispatcherTimer? _timer;
    private int _userId;
    private readonly HashSet<string> _notifiedToday = new();
    private DateTime _lastResetDate = DateTime.Today;

    public NotificationScheduler(
        IHabitService habitService,
        INotificationService notificationService)
    {
        _habitService = habitService;
        _notificationService = notificationService;
    }

    public void Start(int userId)
    {
        Stop();
        _userId = userId;
        _notifiedToday.Clear();
        _lastResetDate = DateTime.Today;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _timer.Tick += async (s, e) => await CheckAsync();
        _timer.Start();

        _ = CheckAsync();
    }

    public void Stop()
    {
        _timer?.Stop();
        _timer = null;
    }

    private async System.Threading.Tasks.Task CheckAsync()
    {
        try
        {
            if (DateTime.Today != _lastResetDate)
            {
                _notifiedToday.Clear();
                _lastResetDate = DateTime.Today;
            }

            var entries = await _habitService.GetEntriesForDateAsync(_userId, DateTime.Today);
            var now = DateTime.Now.TimeOfDay;

            foreach (var entry in entries)
            {
                if (entry.IsCompleted) continue;
                if (entry.ScheduledTime == null) continue;

                var scheduled = entry.ScheduledTime.Value;
                var diff = (now - scheduled).TotalMinutes;

                if (diff >= 0 && diff <= 1)
                {
                    var key = $"{entry.Id}-{entry.Date:yyyyMMdd}-{scheduled:hhmm}";
                    if (_notifiedToday.Contains(key)) continue;

                    _notifiedToday.Add(key);

                    var habitName = entry.Habit?.Name ?? "Привычка";
                    _notificationService.ShowNotification(
                        $"Время привычки! 🌱",
                        $"{habitName} — {scheduled:hh\\:mm}");
                }
            }
        }
        catch
        {
           
        }
    }
}