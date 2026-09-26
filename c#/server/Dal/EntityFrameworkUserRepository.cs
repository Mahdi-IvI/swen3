using Microsoft.EntityFrameworkCore;
using Models;

namespace Dal;

public class EntityFrameworkUserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> FindByUsernameOrEmailAsync(string usernameOrEmail) => db.Users
        .FirstOrDefaultAsync(u => u.Username.ToLower() == usernameOrEmail.ToLower() || u.Email.ToLower() == usernameOrEmail.ToLower());

    public Task<User?> FindByUsernameOrEmailAsync(string username, string email) => db.Users
        .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() || u.Email.ToLower() == email.ToLower());

    public async Task AddAsync(User user)
    {
        try
        {
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new UserAlreadyExistsException(ex);
        }
    }
}
