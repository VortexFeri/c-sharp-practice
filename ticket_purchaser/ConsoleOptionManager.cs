using ticket_purchaser;

internal class ConsoleOptionManager
{
    public static Command? ChooseOne<TCommand>(string title, bool cancelable, List<Command> commands)
    {
        int currentRow = 0;
        ConsoleKey consoleKey;

        Console.CursorVisible = false;

        do
        {
            Console.Clear();
            Console.WriteLine(title);

            int i = 0;
            foreach (var command in commands)
            {
                Console.SetCursorPosition(1 + i % 1, i + 1);

                if (i == currentRow)
                    Console.Write("> ");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(commands.ElementAt(i).Title);
                Console.ResetColor();

                i++;
            }

            consoleKey = Console.ReadKey(true).Key;
            switch (consoleKey)
            {
                case ConsoleKey.DownArrow:
                    if (currentRow + 1 < commands.Count)
                        currentRow++;
                    break;
                case ConsoleKey.UpArrow:
                    if (currentRow >= 1)
                        currentRow--;
                    break;
                case ConsoleKey.Escape:
                    if (cancelable)
                        return default;
                    break;
            }
        } while (consoleKey != ConsoleKey.Enter);

        Console.CursorVisible = true;
        Console.WriteLine("\n");
        return commands.ElementAt(currentRow);
    }

    internal static void Close()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Bye!");
        Console.ResetColor();
        Environment.Exit(0);
    }
}

internal class Command
{
    public string Title { get; set; }

    private readonly Delegate _callback;
    private readonly bool _returnsValue;

    public Command(Action<object[]> action, string title = "")
    {
        _callback = action ?? throw new ArgumentNullException(nameof(action));
        Title = title;
    }

    public Command(Func<object[], object> function, string title = "")
    {
        _callback = function ?? throw new ArgumentNullException(nameof(function));
        Title = title;
    }

    public object? Execute(params object[] parameters)
    {
        if (_callback is Action < object[]> action)
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
