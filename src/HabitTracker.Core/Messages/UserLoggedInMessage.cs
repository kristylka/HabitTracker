namespace HabitTracker.Core.Messages;

using HabitTracker.Core.Models;

public class UserLoggedInMessage
{
    public User User { get; }

    public UserLoggedInMessage(User user)
    {
        User = user;
    }
}