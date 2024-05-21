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

    private static Result<Account, ResultError<PurchaseError>> BuyTicketWrapper(object[] parameters)
    {
        int id = (int)parameters[0];
        return concertManager.BuyTicket(ref user!, id);
    }

    static readonly Command buyCommand = new(BuyTicketWrapper, "Buy Ticket");

    private static void SeeConcerts()
    {
        List<Concert> concerts = concertManager.GetItems();
        ConsoleScreen screen = new([], $"Available Concerts\tBalance: {user!.Balance} Credits");
        foreach(var concert in concerts)
        {
            StringBuilder sb = new();
            sb.Append(concert.Location).Append(", ").
                Append(concert.Date).
                Append("\nTickets available: ").Append(concert.Tickets).
                Append("\nPrice: ").Append(concert.Price).Append(" Credits\n");
            screen.AddMultiLine(concert.Artist, sb.ToString(), _ =>
            {
                ConsoleScreen confirmScreen = new($"Do you want to buy a ticket to this concert for {concert.Price} credits?");
                Command no = new(_ => _, "No");
                Command yes = new(_ =>
                {
                    var res = (Result<Account, ResultError<PurchaseError>>)buyCommand.Execute(concert.Id)!;
                    res.Match(
                        acc =>
                        {
                            Console.WriteLine("Transaction succesful");
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

                return _;
            });

        }
        screen.Cancelable = true;
        screen.Show();
        ConsoleScreen userScreen = new(SetupCommands(), $"Hi, {user.Name}!\t-- Balance: {user.Balance} Credits");
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

                    ConsoleScreen userScreen = new(SetupCommands(), $"Hi, {username}!\t-- Balance: {user.Balance} Credits");
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
            return [seeConcertsCommand, exitCommand];
    }
}
