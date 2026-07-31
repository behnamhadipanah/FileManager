using FileManager.Application.Features.Commands.Folders;
using FluentValidation;

namespace FileManager.Application.Validators.Folders;

public sealed class RenameFolderCommandValidator : AbstractValidator<RenameFolderCommand>
{
    public RenameFolderCommandValidator()
    {
        RuleFor(x => x.ApplicationId).GreaterThan(0);
        RuleFor(x => x.FolderBusinessId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
    }
}
