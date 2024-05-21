﻿namespace ticket_purchaser
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
            int previousRow = -1;
            ConsoleKey consoleKey;

            var pos = Console.GetCursorPosition();

            do
            {
                DisplayMenu(previousRow, currentRow);

                consoleKey = Console.ReadKey(true).Key;
                previousRow = currentRow;
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

            Console.SetCursorPosition(pos.Left, pos.Top + 1);
            Console.WriteLine();
            Commands[currentRow].Execute();
        }

        private void DisplayMenu(int previousRow, int currentRow)
        {
            if (previousRow < 0)
            {
                Console.Clear();
                Console.ForegroundColor = TextColor;
                Console.WriteLine(Title);
                Console.ResetColor();
            }

            Console.CursorVisible = false;

            int totalRows = 0;

            for (int i = 0; i < Commands.Count; i++)
            {
                var command = Commands[i];
                int commandStartRow = totalRows;

                Console.SetCursorPosition(1, commandStartRow + 1);
                if (i == currentRow)
                {
                    Console.Write("> ");
                }

                if (previousRow != currentRow)
                    for (int j = 0; j < command.Rows; j++)
                    {
                        Console.Write(new string(' ', 100));
                        Console.Write(new string('\b', 100));
                    }
                Console.SetCursorPosition(1, commandStartRow + 1);
                totalRows += DisplayCommand(command, commandStartRow, i == currentRow);
            }

            Console.SetCursorPosition(1, 0);
            if (currentRow * (Commands[currentRow].Rows + 1) >= Console.WindowHeight)
                Console.SetCursorPosition(1, currentRow * Commands[currentRow].Rows + Commands[currentRow].Rows - 1);
        }

        private int DisplayCommand(Option command, int startRow, bool isSelected)
        {
            int totalRows = 0;

            var commandLines = command.Title.Split('\n');
            foreach (var line in commandLines)
            {
                Console.SetCursorPosition(isSelected ? 3 : 1, startRow + totalRows + 1);
                Console.ForegroundColor = CommandColor;
                Console.Write(line);
                Console.ResetColor();
                totalRows++;
            }

            if (!string.IsNullOrEmpty(command.Description))
            {
                var descLines = command.Description.Split('\n');
                foreach (var line in descLines)
                {
                    Console.SetCursorPosition(1, startRow + totalRows + 1);
                    Console.ForegroundColor = isSelected ? TextColor : OptionColor;
                    Console.Write(line);
                    Console.ResetColor();
                    totalRows++;
                }
            }

            return totalRows;
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
