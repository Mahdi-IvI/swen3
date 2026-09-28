namespace Bll.@new.Exceptions;

public class DocumentAlreadyExistsException : Exception
{
    public DocumentAlreadyExistsException()
    {
    }

    public DocumentAlreadyExistsException(string? message) : base(message)
    {
    }

    public DocumentAlreadyExistsException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}