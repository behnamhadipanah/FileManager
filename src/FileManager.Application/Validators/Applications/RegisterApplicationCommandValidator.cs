using FileManager.Application.Features.Commands.Applications;
using FluentValidation;

namespace FileManager.Application.Validators.Applications;

public sealed class RegisterApplicationCommandValidator : AbstractValidator<RegisterApplicationCommand>
{
    public RegisterApplicationCommandValidator()
    {
        RuleFor(x => x.ApplicationName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.MinSizeUploadImage).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxSizeUploadImage).GreaterThan(0);
        RuleFor(x => x.MaxSizeUploadImage).GreaterThanOrEqualTo(x => x.MinSizeUploadImage);

        RuleFor(x => x.MinSizeVideo).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxSizeVideo).GreaterThan(0);
        RuleFor(x => x.MaxSizeVideo).GreaterThanOrEqualTo(x => x.MinSizeVideo);

        RuleFor(x => x.MinSizeDcoument).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxSizeDcoument).GreaterThan(0);
        RuleFor(x => x.MaxSizeDcoument).GreaterThanOrEqualTo(x => x.MinSizeDcoument);
    }
}
