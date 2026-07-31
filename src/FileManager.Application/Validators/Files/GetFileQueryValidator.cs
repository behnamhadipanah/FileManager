using FileManager.Application.Features.Queries.Files;
using FluentValidation;

namespace FileManager.Application.Validators.Files;

public sealed class GetFileQueryValidator : AbstractValidator<GetFileQuery>
{
    public GetFileQueryValidator()
    {
        RuleFor(x => x.ApplicationId).GreaterThan(0);
        RuleFor(x => x.FileBusinessId).NotEmpty();
    }
}
