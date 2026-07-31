using FileManager.Application.Features.Commands.Trash;
using FluentValidation;

namespace FileManager.Application.Validators.Trash;

public sealed class RestoreTrashItemCommandValidator : AbstractValidator<RestoreTrashItemCommand>
{
    public RestoreTrashItemCommandValidator()
    {
        RuleFor(x => x.ApplicationId).GreaterThan(0);
        RuleFor(x => x.TrashItemId).GreaterThan(0);
    }
}
