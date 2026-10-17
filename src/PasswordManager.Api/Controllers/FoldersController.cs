using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Api.Common.Extensions;
using PasswordManager.Application.Features.Folders;
using PasswordManager.Application.Features.Folders.Dtos;

namespace PasswordManager.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/vault/folders")]
public class FoldersController : ControllerBase
{
    private readonly IFolderService _folderService;

    public FoldersController(IFolderService folderService)
    {
        _folderService = folderService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FolderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFolders(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var folders = await _folderService.GetFoldersAsync(userId, cancellationToken);
        return Ok(folders);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FolderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFolder(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var folder = await _folderService.GetFolderByIdAsync(id, userId, cancellationToken);
        if (folder == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Folder not found",
                Detail = $"Folder with ID '{id}' was not found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(folder);
    }

    [HttpPost]
    [ProducesResponseType(typeof(FolderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateFolder(
        [FromBody] CreateFolderRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var created = await _folderService.CreateFolderAsync(userId, request, cancellationToken);

        return CreatedAtAction(nameof(GetFolder), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(FolderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateFolder(
        Guid id,
        [FromBody] UpdateFolderRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var updated = await _folderService.UpdateFolderAsync(id, userId, request, cancellationToken);

        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFolder(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var deleted = await _folderService.DeleteFolderAsync(id, userId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
