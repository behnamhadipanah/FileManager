using FileManager.Application.Features.Commands.Applications;
using FluentValidation;

namespace FileManager.Application.Validators.Applications;

public sealed class RegisterApplicationCommandValidator : AbstractValidator<RegisterApplicationCommand>
{
    public RegisterApplicationCommandValidator()
    {
        RuleFor(x => x.ApplicationName).NotEmpty().MaximumLength(200);

        RuleFor(x => x.MinImageSizeKilobytes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxImageSizeKilobytes).GreaterThan(0);
        RuleFor(x => x.MaxImageSizeKilobytes).GreaterThanOrEqualTo(x => x.MinImageSizeKilobytes);

        RuleFor(x => x.MinVideoSizeKilobytes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxVideoSizeKilobytes).GreaterThan(0);
        RuleFor(x => x.MaxVideoSizeKilobytes).GreaterThanOrEqualTo(x => x.MinVideoSizeKilobytes);

        RuleFor(x => x.MinDocumentSizeKilobytes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxDocumentSizeKilobytes).GreaterThan(0);
        RuleFor(x => x.MaxDocumentSizeKilobytes).GreaterThanOrEqualTo(x => x.MinDocumentSizeKilobytes);
    }
}
