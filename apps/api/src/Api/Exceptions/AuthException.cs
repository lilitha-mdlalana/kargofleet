namespace Api.Exceptions;

public class AuthException : Exception
{
    public int Status { get; }

    public AuthException(int status, string message) : base(message)
    {
        Status = status;
    }
}
