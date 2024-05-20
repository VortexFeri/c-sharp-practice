internal class Command
{
    public string Title { get; set; }

    private readonly Delegate _callback;

    public Command(Action<object[]> action, string title = "")
    {
        _callback = action ?? throw new ArgumentNullException(nameof(action));
        Title = title;
    }

    public Command(Func<object[], object?> function, string title = "")
    {
        _callback = function ?? throw new ArgumentNullException(nameof(function));
        Title = title;
    }

    public object? Execute(params object[] parameters)
    {
        if (_callback is Action<object[]> action)
        {
            action(parameters);
            return null;
        }
        if (_callback is Func<object[], object> func)
        {
            return func(parameters);
        }
        throw new InvalidOperationException("Unsupported callback type.");
    }
}
