using FluentValidation;
using PasswordManager.Application.Features.VaultEntries.Dtos;

namespace PasswordManager.Application.Features.VaultEntries.Validators;

public class CreateVaultEntryRequestValidator : AbstractValidator<CreateVaultEntryRequest>
{
    public CreateVaultEntryRequestValidator()
    {
        RuleFor(x => x.Ciphertext)
            .NotEmpty().WithMessage("Ciphertext cannot be empty.");

        RuleFor(x => x.Nonce)
            .NotEmpty().WithMessage("Nonce cannot be empty.");

        RuleFor(x => x.SchemaVersion)
            .GreaterThanOrEqualTo(1).WithMessage("Schema version must be positive.");
    }
}
