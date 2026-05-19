namespace HabitTracker.App.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HabitTracker.Core.Interfaces;
using HabitTracker.Core.Messages;
using HabitTracker.Core.Models;

public partial class CalendarViewModel : ViewModelBase, IRecipient<HabitChangedMessage>
{
    private readonly IHabitService _habitService;
    private readonly IMessenger _messenger;
    private int _userId;

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty]
    private ObservableCollection<HabitEntry> _entries = new();

    [ObservableProperty]
    private ObservableCollection<Habit> _habits = new();

    [ObservableProperty]
    private bool _isLoading;

    // Для создания привычки
    [ObservableProperty]
    private string _newHabitName = string.Empty;

    [ObservableProperty]
    private string _newHabitDescription = string.Empty;

    [ObservableProperty]
    private string _newHabitColor = "#4CAF50";

    [ObservableProperty]
    private TimeSpan _newHabitTime = new(9, 0, 0);

    [ObservableProperty]
    private bool _isAddHabitVisible;

    [ObservableProperty]
    private bool _monday = true;
    [ObservableProperty]
    private bool _tuesday = true;
    [ObservableProperty]
    private bool _wednesday = true;
    [ObservableProperty]
    private bool _thursday = true;
    [ObservableProperty]
    private bool _friday = true;
    [ObservableProperty]
    private bool _saturday;
    [ObservableProperty]
    private bool _sunday;

    public CalendarViewModel(IHabitService habitService, IMessenger messenger)
    {
        _habitService = habitService;
        _messenger = messenger;
        _messenger.Register(this);
    }

    public void SetUser(int userId)
    {
        _userId = userId;
    }

    public void Receive(HabitChangedMessage message)
    {
        _ = LoadEntriesAsync();
    }

    [RelayCommand]
    private async Task LoadEntriesAsync()
    {
        if (_userId == 0) return;

        IsLoading = true;
        try
        {
            var entries = await _habitService.GetEntriesForDateAsync(_userId, SelectedDate);
            Entries = new ObservableCollection<HabitEntry>(entries);

            var habits = await _habitService.GetUserHabitsAsync(_userId);
            Habits = new ObservableCollection<Habit>(habits);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ToggleEntryAsync(HabitEntry entry)
    {
        await _habitService.ToggleEntryCompletionAsync(entry.Id);
        await LoadEntriesAsync();
    }

    [RelayCommand]
    private async Task PreviousDayAsync()
    {
        SelectedDate = SelectedDate.AddDays(-1);
        await LoadEntriesAsync();
    }

    [RelayCommand]
    private async Task NextDayAsync()
    {
        SelectedDate = SelectedDate.AddDays(1);
        await LoadEntriesAsync();
    }

    [RelayCommand]
    private async Task GoToTodayAsync()
    {
        SelectedDate = DateTime.Today;
        await LoadEntriesAsync();
    }

    [RelayCommand]
    private void ShowAddHabit()
    {
        IsAddHabitVisible = !IsAddHabitVisible;
    }

    [RelayCommand]
    private async Task AddHabitAsync()
    {
        if (string.IsNullOrWhiteSpace(NewHabitName)) return;

        var schedules = new System.Collections.Generic.List<(DayOfWeek, TimeSpan, bool)>();

        if (Monday) schedules.Add((DayOfWeek.Monday, NewHabitTime, true));
        if (Tuesday) schedules.Add((DayOfWeek.Tuesday, NewHabitTime, true));
        if (Wednesday) schedules.Add((DayOfWeek.Wednesday, NewHabitTime, true));
        if (Thursday) schedules.Add((DayOfWeek.Thursday, NewHabitTime, true));
        if (Friday) schedules.Add((DayOfWeek.Friday, NewHabitTime, true));
        if (Saturday) schedules.Add((DayOfWeek.Saturday, NewHabitTime, true));
        if (Sunday) schedules.Add((DayOfWeek.Sunday, NewHabitTime, true));

        await _habitService.CreateHabitAsync(
            _userId, NewHabitName, NewHabitDescription, NewHabitColor, schedules);

        NewHabitName = string.Empty;
        NewHabitDescription = string.Empty;
        IsAddHabitVisible = false;

        _messenger.Send(new HabitChangedMessage());
    }

    [RelayCommand]
    private async Task DeleteHabitAsync(Habit habit)
    {
        await _habitService.DeleteHabitAsync(habit.Id);
        _messenger.Send(new HabitChangedMessage());
    }
}