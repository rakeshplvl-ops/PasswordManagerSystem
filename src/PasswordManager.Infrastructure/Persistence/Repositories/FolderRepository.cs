using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Common.Interfaces;
using PasswordManager.Domain.Entities;

namespace PasswordManager.Infrastructure.Persistence.Repositories;

public class FolderRepository : IFolderRepository
{
    private readonly AppDbContext _context;

    public FolderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Folder?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Folders
            .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<Folder>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Folders
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderBy(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Folder folder, CancellationToken cancellationToken = default)
    {
        await _context.Folders.AddAsync(folder, cancellationToken);
    }

    public void Update(Folder folder)
    {
        _context.Folders.Update(folder);
    }

    public void Delete(Folder folder)
    {
        _context.Folders.Remove(folder);
    }
}
