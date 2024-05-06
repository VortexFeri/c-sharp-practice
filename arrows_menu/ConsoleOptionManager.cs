
internal class ConsoleOptionManager
{
    internal static int Choices(string title, bool cancelable, params string[] options)
    {
        int currentSelection = 0;
        ConsoleKey consoleKey;

        Console.CursorVisible = false;

        do
        {
            Console.Clear();
            Console.WriteLine(title);
            for (int i = 0; i < options.Length; i++)
            {
                Console.SetCursorPosition(1 + i % 1, i + 1);
                if (i == currentSelection)
                {
                    Console.Write("> ");
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(options[i]);
                Console.ResetColor();
            }
            consoleKey = Console.ReadKey(true).Key;
            switch (consoleKey)
            {
                case ConsoleKey.DownArrow:
                    {
                        if (currentSelection + 1 < options.Length)
                            currentSelection++;
                        break;
                    }
                case ConsoleKey.UpArrow:
                    {
                        if (currentSelection >= 1)
                            currentSelection--;
                        break;
                    }
                case ConsoleKey.Escape:
                    {
                        if (cancelable)
                            return -1;
                        break;
                    }
            }
        } while (consoleKey != ConsoleKey.Enter);
        Console.CursorVisible = true;
        Console.WriteLine("\n");
        return currentSelection;
    }
}
