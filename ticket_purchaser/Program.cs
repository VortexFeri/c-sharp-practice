using res;
using user_namespace;
using static System.Runtime.InteropServices.JavaScript.JSType;

//TODO: concerts
//TODO: buy tickets
//TODO: registration
//TODO: remove users

namespace ticket_purchaser
{
    internal class Program
    {
        static Account? user;
        static readonly LoginManager loginManager = new("credentials.json");

        static readonly Command loginCommand = new(_ => Login(), "Login");
        static readonly Command exitCommand = new(_ => { Console.ResetColor(); Environment.Exit(0); }, "Exit");
        static readonly Command seeConcertsCommand = new(_ => SeeConcerts(), "See available concerts");

        private static void SeeConcerts()
        {
            throw new NotImplementedException();
        }

        static void Main()
        {
            ConsoleScreen homeScreen = new()
            {
                Title = "Welcome!",
                Commands = SetupCommands()
            };
            while (true)
            {
                homeScreen.Show();
            }
        }

        private static void Login()
        {
            string? PromptUser(string prompt, bool isPassword = false) => ReadInput(prompt, isPassword);

            void HandleResult(Result<Account, ResultError<LoginError>> result, string username)
            {
                result.Match(
                    account =>
                    {
                        // Success
                        user = account;
                        Console.WriteLine("Login successful!");
                        ShowLoadingDots();

                        ConsoleScreen userScreen = new()
                        {
                            Title = "Hi, " + username,
                            Commands = SetupCommands()
                        };
                        userScreen.Show();
                    },
                    error =>
                    {
                        // Failure
                        Console.WriteLine($"{error.Message}");
                        ShowLoadingDots();

                        if (error.Code == LoginError.InvalidPassword)
                        {
                            string? password = PromptUser("Password: ", true);
                            if (!string.IsNullOrEmpty(password))
                            {
                                HandleResult(loginManager.Match(username, password), username);
                            }
                        }
                        else if (error.Code == LoginError.UserNotFound)
                        {
                            Login();
                        }
                    });
            }

            string? username = PromptUser("Username: ");
            if (string.IsNullOrEmpty(username)) return;

            string? password = PromptUser("Password: ", true);
            if (string.IsNullOrEmpty(password)) return;

            HandleResult(loginManager.Match(username, password), username);
        }

        private static List<Command> SetupCommands()
        {
            if (user is null)
                return [loginCommand, exitCommand];

            if (user.Role == Role.User)
                return [seeConcertsCommand, exitCommand];
            else
                return [exitCommand];
        }

        private static void ShowLoadingDots()
        {
            for (int i = 0; i < 3; i++)
            {
                Console.Write(". ");
                Thread.Sleep(500);
            }
            Console.WriteLine("\n");
        }

        private static string? ReadInput(string prompt = "", bool isPassword = false)
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
