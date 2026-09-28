using Bll.@new.Exceptions;
using Dal;
using Models;

namespace Bll.@new;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<User> GetUserByUsernameAsync(string username)
    {
        var user = await userRepository.GetUserByUsernameAsync(username) ??
                   throw new UserNotFoundException($"User with username '{username}' not found.");

        return user;
    }

    public async Task<User> RegisterUserAsync(string username, string hashedPassword,
        string email, string firstName, string lastName)
    {
        username = username.ToLowerInvariant();
        email = email.ToLowerInvariant();

        if (await userRepository.GetUserByUsernameAsync(username) is not null ||
            await userRepository.GetUserByEmailAsync(email) is not null)
        {
            throw new UserAlreadyExistsException("Username or email already exists.");
        }

        var user = new User
        {
            Id=0,
            Username = username,
            HashedPassword = hashedPassword,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        try
        {
            await userRepository.InsertUserAsync(user);
        }
        catch (DuplicateKeyException e)
        {
            throw new UserAlreadyExistsException("Username or email already exists.", e);
        }

        return user;
    }

    public async Task<User> UpdateProfileAsync(string username, string firstName, string lastName, string? hashedPassword)
    {
        var user = await GetUserByUsernameAsync(username);

        user.FirstName = firstName;
        user.LastName = lastName;

        if (!string.IsNullOrWhiteSpace(hashedPassword))
        {
            user.HashedPassword = hashedPassword;
        }

        await userRepository.UpdateUserAsync(user);

        return user;
    }
}
