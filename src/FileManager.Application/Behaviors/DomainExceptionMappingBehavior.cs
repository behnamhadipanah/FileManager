using FileManager.Domain.Exceptions;
using Kootam.Cqrs.Abstractions.Behaviors;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;

namespace FileManager.Application.Behaviors;

/// <summary>
/// Kootam.Cqrs ships its own DomainExceptionBehavior, but it only catches
/// Kootam.Cqrs.Exceptions.DomainStateException. Our aggregates throw
/// FileManager.Domain.Exceptions.DomainException (derived from
/// Kootam.Framework.Domain.Exceptions.DomainStateException instead, to keep the
/// Domain project free of an Application/Cqrs dependency), so we bridge it here.
/// </summary>
public sealed class DomainExceptionMappingBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
    where TResult : Result
{
    public async Task<TResult> Handle(
        TRequest request,
        RequestHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await next();
        }
        catch (DomainException ex)
        {
            var result = Result.Failure(ResultStatus.ValidationError, ex.ToString());
            return (TResult)(object)result;
        }
    }
}
