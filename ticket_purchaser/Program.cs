using System.Text;
using res;
using ticket_purchaser;
using user_namespace;
using static ticket_purchaser.Utils;

//TODO: registration
//TODO: remove users

internal class Program
{
    static Account? user;
    static readonly UserManager loginManager = (UserManager)UserManager.Instance;
    static readonly ConcertManager concertManager = (ConcertManager)ConcertManager.Instance;

    static readonly Command loginCommand = new(_ => Login(), "Login");
    static readonly Command exitCommand = new(_ => { Console.ResetColor(); Environment.Exit(0); }, "Exit");
    static readonly Command seeConcertsCommand = new(_ => SeeConcerts(), "See available concerts");
    static readonly Command buyCommand = new(BuyTicketWrapper, "Buy Ticket");
    static readonly Command seeInventoryCommand = new(_ => SeeInventory(), "See Inventory");

    private static Result<Account, ResultError<PurchaseError>> BuyTicketWrapper(object[] parameters)
    {
        int id = (int)parameters[0];
        return concertManager.BuyTicket(ref user!, id);
    }


    private static void SeeConcerts()
    {
        List<Concert> concerts = concertManager.GetItems();
        ConsoleScreen screen = new([], $"Available Concerts\t--Balance: {user!.Balance} Credits");

        foreach (var concert in concerts)
        {
            StringBuilder sb = new();
            sb.Append($"{concert.Location}, {concert.Date}\n")
              .Append($"Tickets available: {concert.Tickets}\n")
              .Append($"Price: {concert.Price} Credits\n");

            screen.AddMultiLine(concert.Artist, sb.ToString(), _ =>
            {
                ShowConfirmationScreen(concert);
                return _;
            });
        }

        screen.Cancelable = true;
        screen.Show();

        ShowUserScreen();
    }

    private static void SeeInventory()
    {
        if (user!.Concerts.Count != 0)
        {
            Console.WriteLine("\nHere is your inventory\n");
            string header = "Artist".PadRight(20) + "Location".PadRight(20) + "Date".PadRight(12) + "Price".PadRight(10);
            string separator = new('-', header.Length);
            Console.WriteLine(separator);
            Console.WriteLine(header);
            foreach (var id in user!.Concerts)
            {
                var c = concertManager.GetConcertById(id);
                if (c.Value != null)
                {
                    Console.WriteLine(c.Value.PrettyPrint(header.Length));
                }
            }
            Console.WriteLine(separator);
        }
        else
        {
            Console.WriteLine("You do not have any tickets in your inventory.");

        }
        Console.CursorVisible = true;
        Console.WriteLine("Press Enter to Continue...");
        while (Console.ReadKey().Key != ConsoleKey.Enter) ;
    }

    private static void ShowConfirmationScreen(Concert concert)
    {
        ConsoleScreen confirmScreen = new($"Do you want to buy a ticket to this concert for {concert.Price} credits?");
        Command no = new(_ => _, "No");
        Command yes = new(_ =>
        {
            var res = (Result<Account, ResultError<PurchaseError>>)buyCommand.Execute(concert.Id)!;
            res.Match(
                acc =>
                {
                    Console.WriteLine("Transaction successful");
                    ShowLoadingDots();
                },
                err =>
                {
                    Console.WriteLine($"{err.Message}");
                    ShowLoadingDots();
                }
            );
        }, "Yes");

        confirmScreen.AddCommand(no);
        confirmScreen.AddCommand(yes);
        confirmScreen.Show();
    }

    private static void ShowUserScreen()
    {
        ConsoleScreen userScreen = new(SetupCommands(), $"Hi, {user!.Name}!\t-- Balance: {user.Balance} Credits");
        userScreen.Show();
    }

    static void Main()
    {
        UserManager.Initialize("credentials.json");
        ConcertManager.Initialize("concerts.json");
        ConsoleScreen homeScreen = new(SetupCommands(), "Welcome!");

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

                    ShowUserScreen();
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
            return [seeConcertsCommand, seeInventoryCommand, exitCommand];
        else
            return [seeConcertsCommand, seeInventoryCommand, exitCommand];
    }
}
