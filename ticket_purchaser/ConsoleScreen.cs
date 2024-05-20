namespace ticket_purchaser
{
    internal class ConsoleScreen
    {
        private List<Option> Commands { get; set; } = [];
        public string Title;
        public bool Cancelable = false;

        public ConsoleColor CommandColor { get; set; } = ConsoleColor.Green;
        public ConsoleColor TextColor { get; set; } = ConsoleColor.White;
        public ConsoleColor OptionColor { get; set; } = ConsoleColor.DarkGray;

        public ConsoleScreen(string title = "")
        {
            Title = title;
        }

        public ConsoleScreen(List<Command> commands, string title = "")
        {
            foreach (var c in commands)
            {
                Commands.Add(new(c.Title, c));
            }
            Title = title;
        }

        public void AddCommand(Command command, int index)
        {
            Commands.Insert(index, new(command.Title, command));
        }

        public void AddCommand(Command command)
        {
            Commands.Add(new(command.Title, command));
        }

        internal void AddMultiLine(string title, string description, Func<object, object> function)
        {
            Commands.Add(new(title, description, new(function, title)));
        }

        public void ClearCommands()
        {
            foreach (var c in Commands)
            {
                if (c.Description == null)
                {
                    Commands.Remove(c);
                }
            }
        }

        public void ClearCommandsFromView()
        {
            Console.Clear();
            Console.WriteLine(Title);
        }

        public void Show()
        {
            Console.Clear();
            Console.ForegroundColor = TextColor;
            Console.WriteLine(Title);

            ExecuteSelection();
        }

        private void ExecuteSelection()
        {
            int currentRow = 0;
            ConsoleKey consoleKey;

            Console.CursorVisible = false;

            do
            {
                Console.Clear();
                Console.ForegroundColor = TextColor;
                Console.WriteLine(Title);

                int totalRows = 0;

                for (int i = 0; i < Commands.Count; i++)
                {
                    var command = Commands[i];
                    int commandStartRow = totalRows;

                    if (i == currentRow)
                    {
                        Console.SetCursorPosition(1, commandStartRow + 1);
                        Console.Write("> ");
                    }

                    var commandLines = command.Title.Split('\n');
                    for (int line = 0; line < commandLines.Length; line++)
                    {
                        Console.SetCursorPosition(3, commandStartRow + line + 1);

                        Console.ForegroundColor = CommandColor;
                        Console.Write(commandLines[line]);
                        Console.ResetColor();
                        totalRows++;
                    }

                    if (string.IsNullOrEmpty(command.Description))
                        continue;

                    var descLines = command.Description.Split('\n');
                    for (int line = 0; line < descLines.Length; line++)
                    {
                        Console.SetCursorPosition(1, line + commandStartRow + 2);

                        if (i == currentRow)
                            Console.ForegroundColor = ConsoleColor.White;
                        else Console.ForegroundColor = OptionColor;

                        Console.Write(descLines[line]);
                        Console.ResetColor();
                        totalRows++;
                    }
                }

                consoleKey = Console.ReadKey(true).Key;
                switch (consoleKey)
                {
                    case ConsoleKey.DownArrow:
                        if (currentRow + 1 < Commands.Count)
                            currentRow++;
                        break;
                    case ConsoleKey.UpArrow:
                        if (currentRow >= 1)
                            currentRow--;
                        break;
                    case ConsoleKey.Escape:
                        if (Cancelable)
                            return;
                        break;
                }
            } while (consoleKey != ConsoleKey.Enter);

            Console.CursorVisible = true;
            Console.WriteLine("\n");
            Commands[currentRow].Execute();
        }

        private class Option
        {
            public string Title { get; set; }
            public string? Description { get; set; }
            public Command? Callback { private get; set; }

            public Option(string title, string description, Command command)
            {
                Title = title;
                Description = description;
                Callback = command;
            }

            public Option(string title, Command command)
            {
                Title = title;
                Description = null;
                Callback = command;
            }

            public int Rows
            {
                get
                {
                    int rowCount = 1;
                    foreach (var c in Title)
                    {
                        if (c == '\n') rowCount++;
                    }
                    if (Description == null)
                    {
                        return rowCount;
                    }
                    rowCount++;
                    foreach (var c in Description)
                    {
                        if (c == '\n') rowCount++;
                    }
                    return rowCount;
                }
            }

            public object? Execute()
            {
                if (Callback == null)
                    return null;
                return Callback.Execute();
            }
        }
    }
}