using FileManager.Application.Features.Commands.Trash;
using FileManager.Domain.Enumerations;
using FluentValidation;

namespace FileManager.Application.Validators.Trash;

public sealed class MoveToTrashCommandValidator : AbstractValidator<MoveToTrashCommand>
{
    public MoveToTrashCommandValidator()
    {
        RuleFor(x => x.ApplicationId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.ItemType).IsInEnum().Must(x => x is TrashItemType.File or TrashItemType.Folder);
    }
}
