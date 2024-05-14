namespace ticket_purchaser
{
    internal class Program
    {
        static void Main()
        {
            ConsoleScreen homeScreen = new ConsoleScreen();
            Command loginCommand = new Command(_ => Login);
            Command exitCommand = new Command(_ => ConsoleOptionManager.Close());
        }

        static user.Account? Login()
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
