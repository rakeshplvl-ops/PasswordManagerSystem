using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Common.Interfaces;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Infrastructure.Persistence.Repositories;

public class VaultEntryRepository : IVaultEntryRepository
{
    private readonly AppDbContext _context;

    public VaultEntryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<VaultEntry?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.VaultEntries
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<VaultEntry>> GetByUserIdAsync(Guid userId, Guid? folderId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.VaultEntries
            .AsNoTracking()
            .Where(e => e.UserId == userId);

        if (folderId.HasValue)
        {
            query = query.Where(e => e.FolderId == folderId.Value);
        }

        return await query
            .OrderByDescending(e => e.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(VaultEntry entry, CancellationToken cancellationToken = default)
    {
        await _context.VaultEntries.AddAsync(entry, cancellationToken);
    }

    public void Update(VaultEntry entry)
    {
        _context.VaultEntries.Update(entry);
    }

    public void Delete(VaultEntry entry)
    {
        entry.SoftDelete();
        _context.VaultEntries.Update(entry);
    }
}
