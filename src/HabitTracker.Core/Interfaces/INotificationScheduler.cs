namespace HabitTracker.Core.Interfaces;

public interface INotificationScheduler
{
    void Start(int userId);
    void Stop();
}