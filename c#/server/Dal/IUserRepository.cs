using Models;

namespace Dal;

public interface IUserRepository
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task InsertUserAsync(User user);
    Task UpdateUserAsync(User user);
}
