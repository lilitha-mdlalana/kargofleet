namespace Api.Exceptions;

public class TripException : Exception
{
    public int Status { get; }

    public TripException(int status, string message) : base(message)
    {
        Status = status;
    }
}
