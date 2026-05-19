namespace HabitTracker.Core.Models;

public class HabitEntry
{
    public int Id { get; set; }
    public int HabitId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan? ScheduledTime { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Note { get; set; }

    public Habit? Habit { get; set; }
}