namespace ErpLite.Domain.Common;

/// <summary>
/// Raised when a business rule is violated (e.g. issuing more stock than available).
/// The API maps it to a 400 ProblemDetails response.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    public DomainException() { }

    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}
