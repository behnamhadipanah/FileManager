using FileManager.Application.Features.Commands.Files;
using FluentValidation;

namespace FileManager.Application.Validators.Files;

public sealed class RestoreFileCommandValidator : AbstractValidator<RestoreFileCommand>
{
    public RestoreFileCommandValidator()
    {
        RuleFor(x => x.ApplicationId).GreaterThan(0);
        RuleFor(x => x.FileBusinessId).NotEmpty();
    }
}
