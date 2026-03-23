using BusinessAssistant.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessAssistant.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => c.Email).IsUnique();
            entity.HasIndex(c => c.Document).IsUnique();
            entity.Property(c => c.Name).HasMaxLength(200).IsRequired();
            entity.Property(c => c.Email).HasMaxLength(200).IsRequired();
            entity.Property(c => c.Phone).HasMaxLength(20);
            entity.Property(c => c.Document).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Username).HasMaxLength(100).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.PasswordSalt).IsRequired();
            entity.Property(u => u.Role).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.Token).IsUnique();
            entity.Property(r => r.Token).HasMaxLength(256).IsRequired();
            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
