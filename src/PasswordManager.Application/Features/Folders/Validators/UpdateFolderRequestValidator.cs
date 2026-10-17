using FluentValidation;
using PasswordManager.Application.Features.Folders.Dtos;

namespace PasswordManager.Application.Features.Folders.Validators;

public class UpdateFolderRequestValidator : AbstractValidator<UpdateFolderRequest>
{
    public UpdateFolderRequestValidator()
    {
        RuleFor(x => x.EncryptedName)
            .NotEmpty().WithMessage("Encrypted folder name cannot be empty.");

        RuleFor(x => x.Nonce)
            .NotEmpty().WithMessage("Nonce cannot be empty.");
    }
}
