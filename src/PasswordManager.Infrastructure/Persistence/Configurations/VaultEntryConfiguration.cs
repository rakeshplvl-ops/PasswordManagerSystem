using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Infrastructure.Persistence.Configurations;

public class VaultEntryConfiguration : IEntityTypeConfiguration<VaultEntry>
{
    public void Configure(EntityTypeBuilder<VaultEntry> builder)
    {
        builder.ToTable("vault_entries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.FolderId);

        builder.Property(e => e.Ciphertext)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(e => e.Nonce)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.SchemaVersion)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .IsRequired();

        builder.Property(e => e.DeletedAt);

        // Global Query Filter for Soft Delete
        builder.HasQueryFilter(e => e.DeletedAt == null);

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.UserId, e.FolderId });
        builder.HasIndex(e => e.DeletedAt);
    }
}
