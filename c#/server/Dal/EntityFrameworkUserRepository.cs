using Microsoft.EntityFrameworkCore;
using Models;
using Npgsql;

namespace Dal;

public class EntityFrameworkUserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        var normalizedUsername = username.ToLowerInvariant();
        return await dbContext.Users.SingleOrDefaultAsync(u => u.Username.ToLower() == normalizedUsername);
    }

    public Task<User?> GetUserByEmailAsync(string email)
    {
        var normalizedEmail = email.ToLowerInvariant();
        return dbContext.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
    }

    public async Task InsertUserAsync(User user)
    {
        try
        {
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new DuplicateKeyException("Username or email already exists.", e);
        }
    }

    public async Task UpdateUserAsync(User user)
    {
        var existingUser = await GetUserByUsernameAsync(user.Username);
        if (existingUser == null)
        {
            throw new KeyNotFoundException($"User with username '{user.Username}' not found.");
        }

        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.HashedPassword = user.HashedPassword;
        existingUser.Email = user.Email;

        await dbContext.SaveChangesAsync();
    }
}
