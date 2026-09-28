using Microsoft.EntityFrameworkCore;
using Models;

namespace Dal;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(document => { document.HasKey(t => t.Id); });

        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(u => u.Id);

            user.HasMany<Document>().WithOne().HasForeignKey(t => t.Username)
                .HasPrincipalKey(u => u.Username)
                .OnDelete(DeleteBehavior.Cascade);
            user.HasIndex(u => u.Username).IsUnique();
            user.HasIndex(u => u.Email).IsUnique();
            user.Property(u => u.Username).HasMaxLength(100).IsRequired();
            user.Property(u => u.Email).HasMaxLength(320).IsRequired();
            user.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            user.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            user.Property(u => u.HashedPassword).IsRequired();
        });
    }
}
