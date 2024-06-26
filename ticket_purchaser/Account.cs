﻿using System.Text.Json;
using System.Text.Json.Serialization;
using res;
using ticket_purchaser;

namespace user_namespace;

[method: JsonConstructor]
public class Account(string name, string password, List<int> concerts, Role role = Role.User, int balance = 0)
{
    [JsonInclude]
    [JsonPropertyName("name")]
    public string Name { get; } = name;

    [JsonInclude]
    [JsonPropertyName("balance")]
    public int Balance { get; private set; } = balance;

    [JsonInclude]
    [JsonPropertyName("password")]
    private readonly string _password = password;

    [JsonInclude]
    [JsonPropertyName("role")]
    public Role Role { get; } = role;

    [JsonInclude]
    [JsonPropertyName("concerts")]
    public readonly List<int> Concerts = concerts;


    public bool CheckPassword(string password) => _password == password;

    public bool HasTicket(int id)
    {
        return Concerts.Exists(x => x == id);
    }

    public Result<Account, ResultError<UserOperationError>> TryToBuyTicket(in Concert ticket)
    {
        if (Balance <= ticket.Price)
        {
            return new(Error.NotEnoughFunds);
        }
        else
        {
            Concerts.Add((ticket.Id));
            Balance -= ticket.Price;
            return new(this);
        }
    }
}

public class UserManager : SerializableSingleton<Account>
{
    public UserManager(string filePath) : base(filePath)
    {
        Items.Add(new("superdooper24", "myvoiceismypassword", [], Role.Superuser));
        LoadAccounts();
    }

    public static void Initialize(string filePath)
    {
        Initialize(path => new UserManager(path), filePath);
    }

    private bool AccountExists(string username)
    {
        return Items.Exists(acc => acc.Name == username);
    }

    public Result<Account, ResultError<LoginError>> Match(string username, string password)
    {
        if (!AccountExists(username)) return new(Error.UserNotFound);
        var account = Items.Find(acc => acc.Name == username);
        if (account!.CheckPassword(password))
        {
            return new(account);
        }
        else
        {
            return new(Error.InvalidPassword);
        }
    }

    private void LoadAccounts()
    {
        try
        {
            string json = File.ReadAllText(FilePath);
            Account superUser = Items.First();

            Items = JsonSerializer.Deserialize<List<Account>>(json) ?? [];

            Items.Insert(0, superUser);
        }
        catch (JsonException e)
        {
            // If there's an error parsing JSON, accounts list has default content (there is just the Super User account)
            Console.WriteLine("Error loading accounts from file. Initializing empty accounts list.");
            Console.WriteLine(e.Message);
            Utils.ShowLoadingDots();
        }
    }

    private void SaveAccounts()
    {
        string json = JsonSerializer.Serialize(Items.GetRange(1, Items.Count - 1));
        File.WriteAllText(FilePath, json);
    }

    public Result<Account, ResultError<UserOperationError>> BuyTicket(ref Account acc, Concert concert)
    {
        var result = acc.TryToBuyTicket(in concert);
        SaveAccounts();
        return result;
    }

    public Result<Account, ResultError<SignUpError>> Register(string username, string password)
    {
        if (AccountExists(username))
        {
            return new(Error.UserAlreadyExists);
        }

        var newAccount = new Account(username, password, []);
        Items.Add(newAccount);
        SaveAccounts();
        return new(newAccount);
    }
}

public enum Role
{
    User,
    Admin, //TODO: Admin role
    Superuser //TODO: Superuser role
}

public enum SignUpError
{
    UserAlreadyExists,
    InvalidCharacter, //TODO: Check for invalid characters in username
    WeakPassword //TODO: check password strength
}

public enum LoginError
{
    InvalidPassword,
    UserNotFound,
}

public enum UserOperationError
{
    UserNotAuthorized, //TODO: unused
    AdminNotAuthorized, //TODO: unused
    NotEnoughFunds
}

public readonly struct Error
{
    public static readonly ResultError<SignUpError> UserAlreadyExists = new(SignUpError.UserAlreadyExists, "This user already exists.");
    public static readonly ResultError<SignUpError> InvalidCharacter = new(SignUpError.InvalidCharacter, "The username contains invalid characters.");
    public static readonly ResultError<SignUpError> WeakPassword = new(SignUpError.WeakPassword, "The password is too weak.");


    public static readonly ResultError<LoginError> InvalidPassword = new(LoginError.InvalidPassword, "The entered password is invalid.");
    public static readonly ResultError<LoginError> UserNotFound = new(LoginError.UserNotFound, "The user does not exist.");

    public static readonly ResultError<UserOperationError> UserNotAuthorised = new(UserOperationError.UserNotAuthorized, "You are not allowed to do this operation.");
    public static readonly ResultError<UserOperationError> AdminNotAuthorised = new(UserOperationError.AdminNotAuthorized, "Only The SuperUser can do that.");
    public static readonly ResultError<UserOperationError> NotEnoughFunds = new(UserOperationError.NotEnoughFunds, "Insufficient funds.");
}