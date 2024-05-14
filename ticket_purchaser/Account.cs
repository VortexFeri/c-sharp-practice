using System.Text.Json;
using System.Text.Json.Serialization;
using res;

namespace user_namespace
{
    [method: JsonConstructor]
    public class Account(string name, string password, Role role = Role.User)
    {
        [JsonInclude]
        [JsonPropertyName("username")]
        public string Name { get; } = name;

        [JsonInclude]
        [JsonPropertyName("credits")]
        private int Balance { get; }

        [JsonInclude]
        [JsonPropertyName("password")]
        private readonly string Password = password;

        [JsonInclude]
        [JsonPropertyName("role")]
        public readonly Role Role = role;

        public bool CheckPassword(string password) => Password == password;
    }

    public class LoginManager
    {
        private List<Account> accounts = [];
        private readonly string filePath;

        public LoginManager(string filePath)
        {
            this.filePath = filePath;
            LoadAccounts();
        }

        private bool AccountExists(string username)
        {
            return accounts.Exists(acc => acc.Name == username);
        }

        public Result<Account, ResultError<LoginError>> Match(string username, string password)
        {
            if (!AccountExists(username)) return new(Error.UserNotFound);
            var account = accounts.Find(acc => acc.Name == username);
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
                string json = File.ReadAllText(filePath);
                accounts = JsonSerializer.Deserialize<List<Account>>(json) ?? [];
            }
            catch (FileNotFoundException)
            {
                // If the file does not exist, initialize accounts list
                accounts = [];
            }
            catch (JsonException e)
            {
                // If there's an error parsing JSON, initialize accounts list
                Console.WriteLine("Error loading accounts from file. Initializing empty accounts list.");
                Console.WriteLine(e.Message);
                accounts = [];
            }
        }

        private void SaveAccounts()
        {
            string json = JsonSerializer.Serialize(accounts);
            File.WriteAllText(filePath, json);
        }

        public Result<Account, ResultError<SignUpError>> Register(string username, string password)
        {
            if (AccountExists(username))
            {
                Console.WriteLine("Username already exists. Please choose a different username.");
                return new(Error.UserAlreadyExists);
            }

            var newAccount = new Account(username, password);
            accounts.Add(newAccount);
            SaveAccounts();
            Console.WriteLine("Registration successful.");
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
        NotEnoughCredit,
        UserNotAuthorized,
        AdminNotAuthorized
    }

    public readonly struct Error
    {
        public static readonly ResultError<SignUpError> UserAlreadyExists = new(SignUpError.UserAlreadyExists, "This user already exists.");
        public static readonly ResultError<SignUpError> InvalidCharacter = new(SignUpError.InvalidCharacter, "The username contains invalid characters.");
        public static readonly ResultError<SignUpError> WeakPassword = new(SignUpError.WeakPassword, "The password is too weak.");


        public static readonly ResultError<LoginError> InvalidPassword = new(LoginError.InvalidPassword, "The entered password is invalid.");
        public static readonly ResultError<LoginError> UserNotFound = new(LoginError.UserNotFound, "The user does not exist.");

        public static readonly ResultError<UserOperationError> NotEnoughCredit = new(UserOperationError.NotEnoughCredit, "Insufficient funds.");
        public static readonly ResultError<UserOperationError> UserNotAuthorised = new(UserOperationError.UserNotAuthorized, "You are not allowed to do this operation.");
        public static readonly ResultError<UserOperationError> AdminNotAuthorised = new(UserOperationError.AdminNotAuthorized, "Only The SuperUser can do that.");
    }
}
