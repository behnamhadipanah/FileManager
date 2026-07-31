using FileManager.Application.Features.Queries.Folders;
using FluentValidation;

namespace FileManager.Application.Validators.Folders;

public sealed class GetFolderQueryValidator : AbstractValidator<GetFolderQuery>
{
    public GetFolderQueryValidator()
    {
        RuleFor(x => x.ApplicationId).GreaterThan(0);
        RuleFor(x => x.FolderBusinessId).NotEmpty();
    }
}
