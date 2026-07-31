using FileManager.Application.Features.Queries.Folders;
using FluentValidation;

namespace FileManager.Application.Validators.Folders;

public sealed class GetFolderContentsQueryValidator : AbstractValidator<GetFolderContentsQuery>
{
    public GetFolderContentsQueryValidator()
    {
        RuleFor(x => x.ApplicationId).GreaterThan(0);
    }
}
