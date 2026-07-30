using Kootam.Framework.Domain.Exceptions;

namespace FileManager.Domain.Exceptions;

public class DomainException : DomainStateException
{
    public DomainException(string message, params string[] parameters)
        : base(message)
    {
        this.Parameters = parameters;
    }
}
