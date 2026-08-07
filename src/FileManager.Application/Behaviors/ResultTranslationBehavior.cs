using System.Reflection;
using Kootam.Cqrs.Abstractions.Behaviors;
using Kootam.Cqrs.Abstractions.Enums;
using Kootam.Cqrs.Abstractions.Models;
using Kootam.Translator.Abstractions;

namespace FileManager.Application.Behaviors;

/// <summary>
/// Translates <see cref="Result"/> message keys into localized user-facing text.
/// </summary>
public sealed class ResultTranslationBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
    where TResult : Result
{
    private readonly ITranslator _translator;

    public ResultTranslationBehavior(ITranslator translator)
    {
        _translator = translator;
    }

    public async Task<TResult> Handle(
        TRequest request,
        RequestHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        var result = await next();
        if (result.IsSuccess || result.Messages.Count == 0)
            return result;

        var translated = result.Messages
            .Select(TranslateMessage)
            .ToArray();

        return RebuildFailure(result, translated);
    }

    private string TranslateMessage(string message)
        => message.StartsWith("domain.", StringComparison.Ordinal)
            ? _translator.Get(message)
            : message;

    private static TResult RebuildFailure(TResult result, string[] messages)
    {
        var resultType = typeof(TResult);
        if (!resultType.IsGenericType)
            return (TResult)(object)Result.Failure(result.Status, messages);

        var failureMethod = resultType.GetMethod(
            nameof(Result.Failure),
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: [typeof(ResultStatus), typeof(string[])],
            modifiers: null);

        return (TResult)failureMethod!.Invoke(null, [result.Status, messages])!;
    }
}
