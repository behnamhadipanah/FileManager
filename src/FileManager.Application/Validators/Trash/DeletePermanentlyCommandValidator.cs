using FileManager.Application.Features.Commands.Trash;
using FluentValidation;

namespace FileManager.Application.Validators.Trash;

public sealed class DeletePermanentlyCommandValidator : AbstractValidator<DeletePermanentlyCommand>
{
    public DeletePermanentlyCommandValidator()
    {
        RuleFor(x => x.ApplicationId).GreaterThan(0);
        RuleFor(x => x.TrashItemId).GreaterThan(0);
    }
}
