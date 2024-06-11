using System.Text;
using res;
using user_namespace;
using static ticket_purchaser.Utils;

//TODO: registration - partially done - needs to be able to choose a role
//TODO: remove users
//TODO: modify user's funds

namespace ticket_purchaser;

internal class Program
{
    private static void Main()
    {
        UserManager.Initialize("credentials.json");
        ConcertManager.Initialize("concerts.json");
        ConsoleScreen homeScreen = new(SetupCommands(), "Welcome!")
        {
            AutoScroll = false
        };

        while (true)
        {
            homeScreen.Show();
        }
    }

    private static Account? _user;
    private static readonly UserManager LoginManager = (UserManager)UserManager.Instance;
    private static readonly ConcertManager ConcertManager = (ConcertManager)ConcertManager.Instance;

    private static readonly Command LoginCommand = new(_ => Login(), "Login");
    private static readonly Command LogoutCommand = new(_ => { _user = null; }, "Logout");
    private static readonly Command ExitCommand = new(_ => { Console.ResetColor(); Environment.Exit(0); }, "Exit");
    private static readonly Command SeeConcertsCommand = new(_ => SeeConcerts(), "See available concerts");
    private static readonly Command BuyCommand = new(BuyTicketWrapper, "Buy Ticket");
    private static readonly Command SeeInventoryCommand = new(_ => SeeInventory(), "See Inventory");
    private static readonly Command ShowUsersCommand = new(_ => ShowUsers(), "Show Users");
    private static readonly Command AddUserCommand = new(_ => AddUser(), "Register new user");

    private static void AddUser()
    {
        string? username = ReadInput("Username: ");
        if (string.IsNullOrEmpty(username)) return;

        string? password = ReadInput("Password: ", true);
        if (string.IsNullOrEmpty(password)) return;

        HandleResult(LoginManager.Register(username, password), username);
        ShowUserScreen();
        return;

        void HandleResult(Result<Account, ResultError<SignUpError>> result, string inputUsername)
        {
            result.Match(
                _ =>
                {
                    // Success
                    Console.WriteLine("User registered successfully!");
                    ShowLoadingDots();

                    ShowUserScreen();
                },
                error =>
                {
                    // Failure
                    Console.WriteLine($"{error.Message}");
                    ShowLoadingDots();

                    switch (error.Code)
                    {
                        case SignUpError.WeakPassword:
                            {
                                string? inputPassword = ReadInput("Password: ", true);
                                if (!string.IsNullOrEmpty(inputPassword))
                                {
                                    HandleResult(LoginManager.Register(inputUsername, inputPassword), inputUsername);
                                }

                                break;
                            }
                        case SignUpError.InvalidCharacter:
                        case SignUpError.UserAlreadyExists:
                            AddUser();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                });
        }
    }

    private static Result<Account, ResultError<PurchaseError>> BuyTicketWrapper(object[] parameters)
    {
        var id = (int)parameters[0];
        return ConcertManager.BuyTicket(ref _user!, id);
    }


    private static void SeeConcerts()
    {
        List<Concert> concerts = ConcertManager.GetItems();
        ConsoleScreen screen = new([], $"Available Concerts\t--Balance: {_user!.Balance} Credits");

        foreach (var concert in concerts)
        {
            StringBuilder sb = new();
            sb.Append($"{concert.Location}, {concert.Date}\n")
                .Append($"Tickets available: {concert.Tickets}\n")
                .Append($"Price: {concert.Price} Credits\n");

            screen.AddMultiLine(concert.Artist, sb.ToString(), o =>
            {
                PurchaseConfirmationScreen(concert);
                return o;
            });
        }

        screen.Cancelable = true;
        screen.Show();

        ShowUserScreen();
    }

    private static void SeeInventory()
    {
        if (_user!.Concerts.Count != 0)
        {
            Console.WriteLine("\nHere is your inventory\n");
            string header = "Artist".PadRight(20) + "Location".PadRight(20) + "Date".PadRight(12) + "Price".PadLeft(16);
            string separator = new('-', header.Length);
            Console.WriteLine(separator);
            Console.WriteLine(header);
            foreach (var id in _user.Concerts)
            {
                var c = ConcertManager.GetConcertById(id);
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
        while (Console.ReadKey().Key != ConsoleKey.Enter)
        {
        }
        ShowUserScreen();
    }

    private static void PurchaseConfirmationScreen(Concert concert)
    {
        ConsoleScreen confirmScreen = new($"Do you want to buy a ticket to this concert for {concert.Price} credits?");
        Command no = new(o => o, "No");
        Command yes = new(_ =>
        {
            var res = (Result<Account, ResultError<PurchaseError>>)BuyCommand.Execute(concert.Id)!;
            res.Match(
                _ =>
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
        ConsoleScreen userScreen = new(SetupCommands(), $"Hi, {_user!.Name}!\t-- Balance: {_user.Balance} Credits");
        userScreen.Show();
    }

    private static void Login()
    {
        void HandleResult(Result<Account, ResultError<LoginError>> result, string username)
        {
            result.Match(
                account =>
                {
                    // Success
                    _user = account;
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
                        string? password = ReadInput("Password: ", true);
                        if (!string.IsNullOrEmpty(password))
                        {
                            HandleResult(LoginManager.Match(username, password), username);
                        }
                    }
                    else if (error.Code == LoginError.UserNotFound)
                    {
                        Login();
                    }
                });
        }

        string? username = ReadInput("Username: ");
        if (string.IsNullOrEmpty(username)) return;

        string? password = ReadInput("Password: ", true);
        if (string.IsNullOrEmpty(password)) return;

        HandleResult(LoginManager.Match(username, password), username);
    }

    private static List<Command> SetupCommands()
    {
        if (_user is null)
            return [LoginCommand, ExitCommand];

        if (_user.Role == Role.User)
            return [SeeConcertsCommand, SeeInventoryCommand, LogoutCommand, ExitCommand];
        else
            return [SeeConcertsCommand, SeeInventoryCommand, ShowUsersCommand, AddUserCommand, LogoutCommand, ExitCommand];
    }

    private static void ShowUsers()
    {
        List<Account> users = LoginManager.GetItems();
        ConsoleScreen screen = new([], "Registered Users:");

        foreach (var u in users.Where(u => _user != u))
        {
            screen.AddMultiLine(u.Name, $"Balance: {u.Balance}\nRole: {u.Role}", o =>
            {
                if (u.Role == Role.User) return o;
                Console.WriteLine("Only the SUPERUSER can operate onto admins."); // will never show
                ShowLoadingDots();
                return o;
            });
        }

        screen.AutoScroll = false;
        screen.Cancelable = true;
        screen.Show();

        ShowUserScreen();
    }
}
