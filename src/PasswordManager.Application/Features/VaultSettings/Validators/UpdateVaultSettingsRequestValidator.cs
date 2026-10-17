using FluentValidation;
using PasswordManager.Application.Features.VaultSettings.Dtos;

namespace PasswordManager.Application.Features.VaultSettings.Validators;

public class UpdateVaultSettingsRequestValidator : AbstractValidator<UpdateVaultSettingsRequest>
{
    public UpdateVaultSettingsRequestValidator()
    {
        RuleFor(x => x.EncryptedVek)
            .NotEmpty().WithMessage("Encrypted VEK is required.");

        RuleFor(x => x.VekNonce)
            .NotEmpty().WithMessage("VEK nonce is required.");

        RuleFor(x => x.KdfAlgorithm)
            .NotEmpty().WithMessage("KDF algorithm is required.")
            .Equal("Argon2id").WithMessage("Only Argon2id is currently supported.");

        RuleFor(x => x.KdfIterations)
            .GreaterThanOrEqualTo(1).WithMessage("KDF iterations must be at least 1.");

        RuleFor(x => x.KdfMemoryKb)
            .GreaterThanOrEqualTo(1024).WithMessage("KDF memory must be at least 1024 KB.");

        RuleFor(x => x.KdfParallelism)
            .GreaterThanOrEqualTo(1).WithMessage("KDF parallelism must be at least 1.");

        RuleFor(x => x.KdfSalt)
            .NotEmpty().WithMessage("KDF salt is required.");

        RuleFor(x => x.SchemaVersion)
            .GreaterThanOrEqualTo(1).WithMessage("Schema version must be positive.");
    }
}
