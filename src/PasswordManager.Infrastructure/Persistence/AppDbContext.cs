using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Common.Interfaces;
using PasswordManager.Domain.Common;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<VaultSettings> VaultSettings => Set<VaultSettings>();
    public DbSet<VaultEntry> VaultEntries => Set<VaultEntry>();
    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<AuthSession> AuthSessions => Set<AuthSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var utcNow = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.SetUpdatedAt(utcNow);
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
