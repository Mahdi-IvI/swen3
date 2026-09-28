namespace Bll.@new.Exceptions;

public class FolderNotFoundException : Exception
{
    public FolderNotFoundException()
    {
    }

    public FolderNotFoundException(string? message) : base(message)
    {
    }

    public FolderNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}