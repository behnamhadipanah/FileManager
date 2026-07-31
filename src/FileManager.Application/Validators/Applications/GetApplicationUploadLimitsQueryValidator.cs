using FileManager.Application.Features.Queries.Applications;
using FluentValidation;

namespace FileManager.Application.Validators.Applications;

public sealed class GetApplicationUploadLimitsQueryValidator
    : AbstractValidator<GetApplicationUploadLimitsQuery>
{
    public GetApplicationUploadLimitsQueryValidator()
    {
        RuleFor(x => x.ApplicationId).GreaterThan(0);
    }
}
