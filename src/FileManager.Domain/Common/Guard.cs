using System.Runtime.CompilerServices;
using FileManager.Domain.Exceptions;
using FileManager.Domain.Messages;
using Kootam.Framework.Domain.Exceptions;

namespace FileManager.Domain.Common;

public static class Guard
{
    public static string NotEmpty(string? value, [CallerArgumentExpression(nameof(value))] string? field = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainMessages.Required, field);
        return value.Trim();
    }
    
    public static string MinLength(string value, int min, [CallerArgumentExpression(nameof(value))]
        string? field = null)
    {
        if (value.Length < min)
            throw new DomainException(DomainMessages.MinLength, field, min.ToString());
        return value;
    }
    
    public static string MaxLength(string value, int max, [CallerArgumentExpression(nameof(value))] string? field = null)
    {
        if (value.Length > max)
            throw new DomainException(DomainMessages.MaxLength, field, max.ToString());
        return value;
    }
    
    public static T NotNull<T>(T? value, [CallerArgumentExpression(nameof(value))] string? field = null)
        where T : class
        => value ?? throw new DomainException(DomainMessages.Required, field);

    public static int NotNegative(int value, [CallerArgumentExpression(nameof(value))] string? field = null)
    {
        if (value < 0)
            throw new DomainException(DomainMessages.NotNegative, field);
        return value;
    }
    public static int Positive(int value, [CallerArgumentExpression(nameof(value))] string? field = null)
    {
        if (value <= 0)
            throw new DomainException(DomainMessages.Positive, field);
        return value;
    }

    public static long NotNegative(long value, [CallerArgumentExpression(nameof(value))] string? field = null)
    {
        if (value < 0)
            throw new DomainException(DomainMessages.NotNegative, field);
        return value;
    }

    public static long Positive(long value, [CallerArgumentExpression(nameof(value))] string? field = null)
    {
        if (value <= 0)
            throw new DomainException(DomainMessages.Positive, field);
        return value;
    }
}

