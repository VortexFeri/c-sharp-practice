namespace ticket_purchaser
{
    public class Utils
    {
        public static void ShowLoadingDots()
        {
            for (int i = 0; i < 3; i++)
            {
                Console.Write(". ");
                Thread.Sleep(500);
            }
            Console.WriteLine("\n");
        }

        public static string? ReadInput(string prompt = "", bool isPassword = false)
        {
            Console.WriteLine(prompt);
            string input = "";
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    {
                        Console.WriteLine();
                        return null;
                    }
                }
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    if (!string.IsNullOrEmpty(input))
                    {
                        Console.WriteLine();
                        return input;
                    }
                }
                if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (input.Length > 0)
                    {
                        input = input[..^1];
                        Console.Write("\b \b");
                    }
                }
                else if (!char.IsDigit(keyInfo.KeyChar) && !char.IsLetter(keyInfo.KeyChar) && !char.IsSymbol(keyInfo.KeyChar))
                {
                    continue;
                }
                else
                {
                    input += keyInfo.KeyChar;
                    Console.Write(isPassword ? "*" : keyInfo.KeyChar);
                }
            } while (true);
        }

    }
}
