using Models;

namespace Bll.@new;

public interface IUserService
{
    Task<User> GetUserByUsernameAsync(string username);

    Task<User> RegisterUserAsync(string username, string hashedPassword,
        string email, string firstName, string lastName);

    Task<User> UpdateProfileAsync(string username, string firstName, string lastName, string? hashedPassword);
}