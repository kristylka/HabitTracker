namespace HabitTracker.Core.Messages;

public class NavigationMessage
{
    public string ViewName { get; }
    public object? Parameter { get; }

    public NavigationMessage(string viewName, object? parameter = null)
    {
        ViewName = viewName;
        Parameter = parameter;
    }
}