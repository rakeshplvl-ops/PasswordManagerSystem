namespace PasswordManager.Application.Common.Exceptions;

public class DuplicateEmailException : Exception
{
    public DuplicateEmailException(string email)
        : base($"An account with email '{email}' already exists.")
    {
    }
}

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("Invalid email or password.")
    {
    }
}

public class AccountDeactivatedException : Exception
{
    public AccountDeactivatedException()
        : base("This account has been deactivated.")
    {
    }
}
