using FileManager.Application.Features.Queries.Trash;
using FluentValidation;

namespace FileManager.Application.Validators.Trash;

public sealed class GetTrashContentsQueryValidator : AbstractValidator<GetTrashContentsQuery>
{
    public GetTrashContentsQueryValidator()
    {
        RuleFor(x => x.ApplicationId).GreaterThan(0);
    }
}
