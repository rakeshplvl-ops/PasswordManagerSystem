using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Infrastructure.Persistence.Configurations;

public class VaultSettingsConfiguration : IEntityTypeConfiguration<VaultSettings>
{
    public void Configure(EntityTypeBuilder<VaultSettings> builder)
    {
        builder.ToTable("vault_settings");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.UserId)
            .IsRequired();

        builder.HasIndex(v => v.UserId)
            .IsUnique();

        builder.Property(v => v.EncryptedVek)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(v => v.VekNonce)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(v => v.KdfAlgorithm)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(v => v.KdfIterations)
            .IsRequired();

        builder.Property(v => v.KdfMemoryKb)
            .IsRequired();

        builder.Property(v => v.KdfParallelism)
            .IsRequired();

        builder.Property(v => v.KdfSalt)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(v => v.SchemaVersion)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(v => v.CreatedAt)
            .IsRequired();

        builder.Property(v => v.UpdatedAt)
            .IsRequired();
    }
}
