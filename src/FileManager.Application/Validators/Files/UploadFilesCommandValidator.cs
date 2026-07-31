using FileManager.Application.Features.Commands.Files;
using FluentValidation;

namespace FileManager.Application.Validators.Files;

public sealed class UploadFilesCommandValidator : AbstractValidator<UploadFilesCommand>
{
    public UploadFilesCommandValidator()
    {
        RuleFor(x => x.ApplicationId).GreaterThan(0);
        RuleFor(x => x.Files).NotEmpty();
        RuleForEach(x => x.Files).ChildRules(file =>
        {
            file.RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
            file.RuleFor(x => x.Content).NotNull();
        });
    }
}
