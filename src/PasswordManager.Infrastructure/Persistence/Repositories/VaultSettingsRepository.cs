using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Common.Interfaces;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Infrastructure.Persistence.Repositories;

public class VaultSettingsRepository : IVaultSettingsRepository
{
    private readonly AppDbContext _context;

    public VaultSettingsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<VaultSettings?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.VaultSettings
            .FirstOrDefaultAsync(v => v.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(VaultSettings settings, CancellationToken cancellationToken = default)
    {
        await _context.VaultSettings.AddAsync(settings, cancellationToken);
    }

    public void Update(VaultSettings settings)
    {
        _context.VaultSettings.Update(settings);
    }
}
