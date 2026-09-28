namespace Bll.@new.Exceptions;

public class FolderAlreadyExistsException : Exception
{
    public FolderAlreadyExistsException()
    {
    }

    public FolderAlreadyExistsException(string? message) : base(message)
    {
    }

    public FolderAlreadyExistsException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}