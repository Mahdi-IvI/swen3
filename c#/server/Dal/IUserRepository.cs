using Models;

namespace Dal;

public interface IUserRepository
{
    Task<User?> FindByUsernameOrEmailAsync(string usernameOrEmail);
    Task<User?> FindByUsernameOrEmailAsync(string username, string email);
    Task AddAsync(User user);
}

public sealed class UserAlreadyExistsException : Exception
{
    public UserAlreadyExistsException(Exception innerException) : base("A user with this username or email already exists.", innerException) { }
}
