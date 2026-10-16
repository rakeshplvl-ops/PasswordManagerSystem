using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Infrastructure.Persistence.Configurations;

public class AuthSessionConfiguration : IEntityTypeConfiguration<AuthSession>
{
    public void Configure(EntityTypeBuilder<AuthSession> builder)
    {
        builder.ToTable("auth_sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.TokenHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(s => s.TokenHash)
            .IsUnique();

        builder.Property(s => s.ExpiresAt)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .IsRequired();

        builder.Property(s => s.RevokedAt);

        builder.Property(s => s.IpAddress)
            .HasMaxLength(64);

        builder.Property(s => s.UserAgent)
            .HasMaxLength(512);

        builder.HasIndex(s => new { s.UserId, s.ExpiresAt });
    }
}
