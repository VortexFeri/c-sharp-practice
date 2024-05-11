using System.Runtime.Serialization;
using System.Text.Json;
using LoginException;

namespace LoginException
{
    public class UserNotFoundException(string user) : Exception($"User {user} does not exist")
    {
    }

    public class InvalidPasswordException(string message) : Exception(message)
    {
    }

    public class FileEmptyException(string filepath) : Exception($"File {filepath} is empty")
    {
    }
}

internal class Program
{
    static void Main()
    {
        Console.Title = "Exercise 2: Login Menu";
        HandleLoggedOutUser();
    }

    static void HandleLoggedOutUser()
    {
        int selected = ConsoleOptionManager.Choices("Welcome!", true, "Login", "Exit");
        switch (selected)
        {
            case 0:
                {
                    try
                    {
                        string user = HandleLogin(GetCredentials(filepath: "credentials.json"));
                        HandleLoggedInUser(user);
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine();
                        HandleLoggedOutUser();
                    }
                    catch (JsonException ex)
                    {
                        Console.Write(ex.Message);
                        return;
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.Message);
                        for (int i = 1; i <= 3; i++)
                        {
                            Thread.Sleep(500);
                            Console.Write(".");

                        }
                        HandleLoggedOutUser();
                    }
                    break;
                }
            case 1:
                {
                    ConsoleOptionManager.Close();
                    break;
                }
        }
    }

    static void HandleLoggedInUser(string username)
    {
        int selected = ConsoleOptionManager.Choices($"Welcome, {username}!", false, "Show Users", "Logout", "Exit");
        switch (selected)
        {
            case 0:
                {
                    ConsoleKey key;
                    Console.WriteLine("Registered Users:");
                    Dictionary<string, string> users = GetCredentials("credentials.json");
                    Console.WriteLine("-------------------");
                    foreach (string user in users.Keys)
                    {
                        Console.WriteLine(user);
                    }
                    Console.WriteLine("-------------------");
                    Console.Write("Press Enter to continue...");
                    do
                    {
                        key = Console.ReadKey(true).Key;
                    } while (key != ConsoleKey.Enter && key != ConsoleKey.Escape);
                    HandleLoggedInUser(username);
                    break;
                }
            case 1:
                {
                    HandleLoggedOutUser();
                    break;
                }
            case 2:
                {
                    ConsoleOptionManager.Close();
                    break;
                }
        }
    }

    private static Dictionary<string, string> GetCredentials(string filepath)
    {
        if (!File.Exists(filepath))
        {
            throw new FileNotFoundException("Credentials file does not exist");
        }

        string jsonContent = File.ReadAllText(filepath);

        if (string.IsNullOrEmpty(jsonContent))
        {
            throw new LoginException.FileEmptyException(filepath);
        }

        return JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent) ?? [];
    }

    private static string HandleLogin(Dictionary<string, string> credentials)
    {
        Console.Write("Enter username: ");
        string userInput = ReadInput() ?? throw new OperationCanceledException();

        if (string.IsNullOrWhiteSpace(userInput))
        {
            Console.Write("Please enter a valid username");
            return HandleLogin(credentials);
        }

        if (!credentials.TryGetValue(userInput, out string? pass))
        {
            throw new LoginException.UserNotFoundException(userInput);
        }

        Console.Write("Enter password: ");
        string? passInput;
        do
        {
            passInput = ReadInput(true) ?? throw new OperationCanceledException();
            if (!string.IsNullOrEmpty(passInput)) break;
        } while (string.IsNullOrEmpty(passInput));

        if (pass == passInput)
        {
            return userInput;
        }
        else
        {
            throw new InvalidPasswordException("Invalid password");
        }
    }

    private static string? ReadInput(bool isPassword = false)
    {
        string input = "";
        ConsoleKeyInfo keyInfo;
        do
        {
            keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.Escape)
            {
                Console.WriteLine();
                return null;
            }
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return input;
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
