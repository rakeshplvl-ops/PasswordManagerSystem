using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Api.Common.Extensions;
using PasswordManager.Application.Features.VaultSettings;
using PasswordManager.Application.Features.VaultSettings.Dtos;

namespace PasswordManager.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/vault/settings")]
public class VaultSettingsController : ControllerBase
{
    private readonly IVaultSettingsService _vaultSettingsService;

    public VaultSettingsController(IVaultSettingsService vaultSettingsService)
    {
        _vaultSettingsService = vaultSettingsService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(VaultSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var settings = await _vaultSettingsService.GetSettingsAsync(userId, cancellationToken);
        if (settings == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Vault settings not found",
                Detail = "No cryptographic vault settings have been initialized for this account.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(settings);
    }

    [HttpPut]
    [ProducesResponseType(typeof(VaultSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateSettings(
        [FromBody] UpdateVaultSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var updated = await _vaultSettingsService.UpdateSettingsAsync(userId, request, cancellationToken);
        return Ok(updated);
    }
}
