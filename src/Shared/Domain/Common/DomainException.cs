namespace IcePlant.Domain.Common;

/// <summary>
/// Thrown when a domain invariant is violated programmatically
/// (i.e. the caller bypassed the Result pattern).
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
