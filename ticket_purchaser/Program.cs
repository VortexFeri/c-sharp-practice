using user;

namespace ticket_purchaser
{
    internal class Program
    {
        static void Main()
        {
            ConsoleScreen homeScreen = new();
            Command loginCommand = new(_ => Login(), "Login");
            Command exitCommand = new Command(_ =>
            {
                Console.ResetColor();
                Environment.Exit(0);
            });

            homeScreen.Title = "Welcome!";
            homeScreen.AddCommand(ref loginCommand);
            while (true)
            {
                homeScreen.Show();
            }
        }

        static Account? Login()
        {
            return null;
        }

        static void Logout()
        {
            throw new NotImplementedException();
        }

        static void ShowUsers()
        {
            throw new NotImplementedException();
        }
    }
}
