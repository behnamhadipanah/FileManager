using FileManager.Application.Features.Queries.Folders;
using FluentValidation;

namespace FileManager.Application.Validators.Folders;

public sealed class GetManageFolderQueryValidator : AbstractValidator<GetManageFolderQuery>
{
    public GetManageFolderQueryValidator()
    {
        RuleFor(x => x.ApplicationBusinessId).NotEmpty();
    }
}
