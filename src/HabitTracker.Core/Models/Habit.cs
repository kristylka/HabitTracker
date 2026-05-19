namespace HabitTracker.Core.Models;

public class Habit
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "#4CAF50";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public User? User { get; set; }
    public List<HabitEntry> Entries { get; set; } = new();
    public List<HabitSchedule> Schedules { get; set; } = new();
}