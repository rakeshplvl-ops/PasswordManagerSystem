using FluentValidation;
using PasswordManager.Application.Features.Auth.Dtos;

namespace PasswordManager.Application.Features.Auth.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(10).WithMessage("Password must be at least 10 characters long.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character.");

        When(x => x.VaultSettings != null, () =>
        {
            RuleFor(x => x.VaultSettings!.EncryptedVek)
                .NotEmpty().WithMessage("Encrypted VEK is required when vault settings are provided.");

            RuleFor(x => x.VaultSettings!.VekNonce)
                .NotEmpty().WithMessage("VEK nonce is required when vault settings are provided.");

            RuleFor(x => x.VaultSettings!.KdfSalt)
                .NotEmpty().WithMessage("KDF salt is required when vault settings are provided.");

            RuleFor(x => x.VaultSettings!.KdfAlgorithm)
                .NotEmpty().WithMessage("KDF algorithm is required.");

            RuleFor(x => x.VaultSettings!.KdfIterations)
                .GreaterThanOrEqualTo(1).WithMessage("KDF iterations must be at least 1.");

            RuleFor(x => x.VaultSettings!.KdfMemoryKb)
                .GreaterThanOrEqualTo(1024).WithMessage("KDF memory must be at least 1024 KB.");

            RuleFor(x => x.VaultSettings!.KdfParallelism)
                .GreaterThanOrEqualTo(1).WithMessage("KDF parallelism must be at least 1.");
        });
    }
}
