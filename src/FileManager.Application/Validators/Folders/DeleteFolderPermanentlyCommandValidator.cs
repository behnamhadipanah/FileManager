using FileManager.Application.Features.Commands.Folders;
using FluentValidation;

namespace FileManager.Application.Validators.Folders;

public sealed class DeleteFolderPermanentlyCommandValidator : AbstractValidator<DeleteFolderPermanentlyCommand>
{
    public DeleteFolderPermanentlyCommandValidator()
    {
        RuleFor(x => x.ApplicationId).GreaterThan(0);
        RuleFor(x => x.FolderBusinessId).NotEmpty();
    }
}
