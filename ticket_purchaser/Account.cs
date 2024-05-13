using res;

namespace user
{
    public class Account(string name, string password, Account.Roles role = Account.Roles.User)
    {
        public string Name { get; } = name;
        private int Balance { get; } = 0;
        private readonly string Password = password;
        private readonly Roles Role = role;

        public bool CheckPassword(string password) => Password != password;

        public enum Roles
        {
            Admin,
            User,
            Superuser
        }

        readonly struct Error
        {
            internal enum UserError
            {
                UserNotFound,
                UserAlreadyExists,
                UserNotAuthorized,
                InvalidPassword
            }
            public static readonly Error<UserError> UserNotFound = new(UserError.UserNotFound, "The user does not exist.");
            public static readonly Error<UserError> UserAlreadyExists = new(UserError.UserAlreadyExists, "This user already exists");
            public static readonly Error<UserError> UserNotAuthorised = new(UserError.UserNotAuthorized, "You are not allowed to do this operation.");
            public static readonly Error<UserError> InvalidPassword = new(UserError.InvalidPassword, "The entered password is invalid");
        }
    }
}
