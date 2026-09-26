using Microsoft.EntityFrameworkCore;
using Models;

namespace Dal;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<User>();
        user.HasKey(u => u.Id);
        user.HasIndex(u => u.Username).IsUnique();
        user.HasIndex(u => u.Email).IsUnique();
        user.Property(u => u.Username).HasMaxLength(100).IsRequired();
        user.Property(u => u.Email).HasMaxLength(320).IsRequired();
        user.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        user.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        user.Property(u => u.HashedPassword).IsRequired();
    }
}
