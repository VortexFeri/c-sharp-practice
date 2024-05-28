using System;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using res;
using ticket_purchaser;
using user_namespace;
using static ticket_purchaser.Utils;

//TODO: registration - partially done - needs to be able to choose a role
//TODO: remove users
//TODO: modify user's funds

internal class Program
{
    static void Main()
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

    static Account? user;
    static readonly UserManager loginManager = (UserManager)UserManager.Instance;
    static readonly ConcertManager concertManager = (ConcertManager)ConcertManager.Instance;

    static readonly Command loginCommand = new(_ => Login(), "Login");
    static readonly Command logoutCommand = new(_ => { user = null; }, "Logout");
    static readonly Command exitCommand = new(_ => { Console.ResetColor(); Environment.Exit(0); }, "Exit");
    static readonly Command seeConcertsCommand = new(_ => SeeConcerts(), "See available concerts");
    static readonly Command buyCommand = new(BuyTicketWrapper, "Buy Ticket");
    static readonly Command seeInventoryCommand = new(_ => SeeInventory(), "See Inventory");
    static readonly Command showUsersCommand = new(_ => ShowUsers(), "Show Users");
    static readonly Command addUserCommand = new(_ => AddUser(), "Register new user");

    private static void AddUser()
    {
        void HandleResult(Result<Account, ResultError<SignUpError>> result, string username)
        {
            result.Match(
                account =>
                {
                    // Success
                    Console.WriteLine("User registered succesfully!");
                    ShowLoadingDots();

                    ShowUserScreen();
                },
                error =>
                {
                    // Failure
                    Console.WriteLine($"{error.Message}");
                    ShowLoadingDots();

                    if (error.Code == SignUpError.WeakPassword)
                    {
                        string? password = ReadInput("Password: ", true);
                        if (!string.IsNullOrEmpty(password))
                        {
                            HandleResult(loginManager.Register(username, password), username);
                        }
                    }
                    else if (error.Code == SignUpError.InvalidCharacter || error.Code == SignUpError.UserAlreadyExists)
                    {
                        AddUser();
                    }
                });
        }

        string? username = ReadInput("Username: ");
        if (string.IsNullOrEmpty(username)) return;

        string? password = ReadInput("Password: ", true);
        if (string.IsNullOrEmpty(password)) return;

        HandleResult(loginManager.Register(username, password), username);
        ShowUserScreen();
    }

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
                PurchaseConfirmationScreen(concert);
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
            string header = "Artist".PadRight(20) + "Location".PadRight(20) + "Date".PadRight(12) + "Price".PadLeft(16);
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
        ShowUserScreen();
    }

    private static void PurchaseConfirmationScreen(Concert concert)
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

    private static void Login()
    {
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
                        string? password = ReadInput("Password: ", true);
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

        string? username = ReadInput("Username: ");
        if (string.IsNullOrEmpty(username)) return;

        string? password = ReadInput("Password: ", true);
        if (string.IsNullOrEmpty(password)) return;

        HandleResult(loginManager.Match(username, password), username);
    }

    private static List<Command> SetupCommands()
    {
        if (user is null)
            return [loginCommand, exitCommand];

        if (user.Role == Role.User)
            return [seeConcertsCommand, seeInventoryCommand, logoutCommand, exitCommand];
        else
            return [seeConcertsCommand, seeInventoryCommand, showUsersCommand, addUserCommand, logoutCommand, exitCommand];
    }

    private static void ShowUsers()
    {
        List<Account> users = loginManager.GetItems();
        ConsoleScreen screen = new([], $"Registered Users:");

        foreach (var u in users)
        {
            if (user == u) continue;

            screen.AddMultiLine(u.Name, $"Balance: {u.Balance}\nRole: {u.Role}", _ =>
            {
                if (u.Role != Role.User)
                {
                    Console.WriteLine("Only the SUPERUSER can operate onto admins."); // will never show
                    ShowLoadingDots();
                    return _;
                }

                //ConsoleScreen confirmScreen = new($"What do you want to do to the user {u.Name}?");
                //Command cancel = new(_ => _, "Cancel");
                //Command remove = new(_ =>
                //{
                //    var res = (Result<Account, ResultError<UserOperationError>>)removeCommand.Execute(u.Name)!;
                //    res.Match(
                //        acc =>
                //        {
                //            Console.WriteLine("Transaction successful");
                //            ShowLoadingDots();
                //        },
                //        err =>
                //        {
                //            Console.WriteLine($"{err.Message}");
                //            ShowLoadingDots();
                //        }
                //    );
                //    return _;
                //});
                return _;
            });
        }

        screen.AutoScroll = false;
        screen.Cancelable = true;
        screen.Show();

        ShowUserScreen();
    }
}
