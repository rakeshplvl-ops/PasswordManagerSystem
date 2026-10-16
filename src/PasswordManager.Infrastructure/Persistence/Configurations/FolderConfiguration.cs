using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Infrastructure.Persistence.Configurations;

public class FolderConfiguration : IEntityTypeConfiguration<Folder>
{
    public void Configure(EntityTypeBuilder<Folder> builder)
    {
        builder.ToTable("folders");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.UserId)
            .IsRequired();

        builder.HasIndex(f => f.UserId);

        builder.Property(f => f.EncryptedName)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(f => f.Nonce)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .IsRequired();

        builder.Property(f => f.UpdatedAt)
            .IsRequired();

        builder.HasMany(f => f.VaultEntries)
            .WithOne(e => e.Folder)
            .HasForeignKey(e => e.FolderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
