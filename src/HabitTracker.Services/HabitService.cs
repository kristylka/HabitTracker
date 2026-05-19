namespace HabitTracker.Services;

using HabitTracker.Core.Interfaces;
using HabitTracker.Core.Models;

public class HabitService : IHabitService
{
    private readonly IHabitRepository _habitRepo;
    private readonly IHabitEntryRepository _entryRepo;
    private readonly IHabitScheduleRepository _scheduleRepo;

    public HabitService(
        IHabitRepository habitRepo,
        IHabitEntryRepository entryRepo,
        IHabitScheduleRepository scheduleRepo)
    {
        _habitRepo = habitRepo;
        _entryRepo = entryRepo;
        _scheduleRepo = scheduleRepo;
    }

    public async Task<List<Habit>> GetUserHabitsAsync(int userId)
        => await _habitRepo.GetByUserIdAsync(userId);

    public async Task<Habit> CreateHabitAsync(int userId, string name, string description,
        string color, List<(DayOfWeek Day, TimeSpan Time, bool Notify)> schedules)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название привычки не может быть пустым");

        var habit = new Habit
        {
            UserId = userId,
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty,
            Color = color,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _habitRepo.CreateAsync(habit);

        foreach (var (day, time, notify) in schedules)
        {
            var schedule = new HabitSchedule
            {
                HabitId = habit.Id,
                DayOfWeek = day,
                Time = time,
                NotifyEnabled = notify
            };
            await _scheduleRepo.CreateAsync(schedule);
        }

        return habit;
    }

    public async Task UpdateHabitAsync(Habit habit)
        => await _habitRepo.UpdateAsync(habit);

    public async Task DeleteHabitAsync(int habitId)
        => await _habitRepo.DeleteAsync(habitId);

    public async Task GenerateEntriesForDateAsync(int userId, DateTime date)
    {
        var dayOfWeek = date.DayOfWeek;
        var schedules = await _scheduleRepo.GetByUserAndDayAsync(userId, dayOfWeek);

        foreach (var schedule in schedules)
        {
            var existing = await _entryRepo.GetByHabitAndDateAsync(
                schedule.HabitId, date, schedule.Time);

            if (existing == null)
            {
                var entry = new HabitEntry
                {
                    HabitId = schedule.HabitId,
                    Date = date.Date,
                    ScheduledTime = schedule.Time,
                    IsCompleted = false
                };
                await _entryRepo.CreateAsync(entry);
            }
        }
    }

    public async Task<List<HabitEntry>> GetEntriesForDateAsync(int userId, DateTime date)
    {
        await GenerateEntriesForDateAsync(userId, date);
        return await _entryRepo.GetByDateAsync(userId, date);
    }

    public async Task<List<HabitEntry>> GetEntriesForRangeAsync(int userId, DateTime from, DateTime to)
        => await _entryRepo.GetByDateRangeAsync(userId, from, to);

    public async Task ToggleEntryCompletionAsync(int entryId)
    {
        var entry = await _entryRepo.GetByIdAsync(entryId);
        if (entry == null)
            throw new InvalidOperationException("Запись не найдена");

        entry.IsCompleted = !entry.IsCompleted;
        entry.CompletedAt = entry.IsCompleted ? DateTime.UtcNow : null;
        await _entryRepo.UpdateAsync(entry);
    }
}