using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Api.Common.Extensions;
using PasswordManager.Application.Features.VaultEntries;
using PasswordManager.Application.Features.VaultEntries.Dtos;

namespace PasswordManager.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/vault/entries")]
public class VaultEntriesController : ControllerBase
{
    private readonly IVaultEntryService _vaultEntryService;

    public VaultEntriesController(IVaultEntryService vaultEntryService)
    {
        _vaultEntryService = vaultEntryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VaultEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetEntries(
        [FromQuery] Guid? folderId,
        [FromQuery] bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var entries = await _vaultEntryService.GetEntriesAsync(userId, folderId, includeDeleted, cancellationToken);
        return Ok(entries);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VaultEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEntry(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var entry = await _vaultEntryService.GetEntryByIdAsync(id, userId, cancellationToken);
        if (entry == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Vault entry not found",
                Detail = $"Vault entry with ID '{id}' was not found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(entry);
    }

    [HttpPost]
    [ProducesResponseType(typeof(VaultEntryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateEntry(
        [FromBody] CreateVaultEntryRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var created = await _vaultEntryService.CreateEntryAsync(userId, request, cancellationToken);

        return CreatedAtAction(nameof(GetEntry), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(VaultEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateEntry(
        Guid id,
        [FromBody] UpdateVaultEntryRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var updated = await _vaultEntryService.UpdateEntryAsync(id, userId, request, cancellationToken);

        return Ok(updated);
    }

    [HttpPut("{id:guid}/move")]
    [ProducesResponseType(typeof(VaultEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MoveEntry(
        Guid id,
        [FromBody] MoveVaultEntryRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var moved = await _vaultEntryService.MoveEntryAsync(id, userId, request, cancellationToken);

        return Ok(moved);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SoftDeleteEntry(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var deleted = await _vaultEntryService.SoftDeleteEntryAsync(id, userId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/restore")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreEntry(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var restored = await _vaultEntryService.RestoreEntryAsync(id, userId, cancellationToken);
        if (!restored)
        {
            return NotFound();
        }

        return Ok();
    }

    [HttpDelete("{id:guid}/purge")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PurgeEntry(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var purged = await _vaultEntryService.PurgeEntryAsync(id, userId, cancellationToken);
        if (!purged)
        {
            return NotFound();
        }

        return NoContent();
    }
}
