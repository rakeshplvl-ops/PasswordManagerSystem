using FluentValidation;
using PasswordManager.Application.Features.Folders.Dtos;

namespace PasswordManager.Application.Features.Folders.Validators;

public class CreateFolderRequestValidator : AbstractValidator<CreateFolderRequest>
{
    public CreateFolderRequestValidator()
    {
        RuleFor(x => x.EncryptedName)
            .NotEmpty().WithMessage("Encrypted folder name cannot be empty.");

        RuleFor(x => x.Nonce)
            .NotEmpty().WithMessage("Nonce cannot be empty.");
    }
}
