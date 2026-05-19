namespace HabitTracker.Core.Models;

public class HabitSchedule
{
    public int Id { get; set; }
    public int HabitId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan Time { get; set; }
    public bool NotifyEnabled { get; set; } = true;

    public Habit? Habit { get; set; }
}