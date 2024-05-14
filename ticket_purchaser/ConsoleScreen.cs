namespace ticket_purchaser
{
    internal class ConsoleScreen
    {
        public List<Command> Commands { private get; set; } = [];
        public string Title;
        public bool Cancelable = false;

        public ConsoleColor CommandColor { get; set; } = ConsoleColor.Green;
        public ConsoleColor TextColor { get; set; } = ConsoleColor.White;

        public ConsoleScreen(string title = "")
        {
            Title = title;
        }

        public ConsoleScreen(ref List<Command> commands, string title = "")
        {
            Commands = commands;
            Title = title;
        }

        public void AddCommand(ref Command command, int index)
        {
            Commands.Insert(index, command);
        }

        public void AddCommand(ref Command command)
        {
            Commands.Add(command);
        }

        public void ClearCommands()
        {
            Commands.Clear();
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
            if (Commands.Count != 0)
            {
                if (ExecuteSelection())
                    Show();
            }
        }

        private bool ExecuteSelection()
        {

            int currentRow = 0;
            ConsoleKey consoleKey;

            Console.CursorVisible = false;

            do
            {
                Console.Clear();
                Console.ForegroundColor = TextColor;
                Console.WriteLine(Title);

                int i = 0;
                foreach (var command in Commands)
                {
                    Console.SetCursorPosition(1 + i % 1, i + 1);

                    if (i == currentRow)
                        Console.Write("> ");

                    Console.ForegroundColor = CommandColor;
                    if (string.IsNullOrEmpty(command.Title))
                    {
                        command.Title = "Command " + (i + 1);
                    }
                    Console.Write(command.Title);
                    Console.ResetColor();

                    i++;
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
                            return false;
                        break;
                }
            } while (consoleKey != ConsoleKey.Enter);

            Console.CursorVisible = true;
            Console.WriteLine("\n");
            Commands.ElementAt(currentRow).Execute();
            return true;
        }
    }
}