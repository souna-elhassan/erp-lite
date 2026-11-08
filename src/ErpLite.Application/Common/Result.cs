namespace ErpLite.Application.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
}

public sealed record Error(ErrorType Type, string Message)
{
    public static Error NotFound(string message) => new(ErrorType.NotFound, message);
    public static Error Conflict(string message) => new(ErrorType.Conflict, message);
    public static Error Validation(string message) => new(ErrorType.Validation, message);
}

/// <summary>
/// Outcome of a use case: either a value or an expected error (not found, conflict, validation).
/// Unexpected failures still throw.
/// </summary>
public sealed class Result<T>
{
    private Result(T? value, Error? error)
    {
        Value = value;
        Error = error;
    }

    public T? Value { get; }
    public Error? Error { get; }
    public bool IsSuccess => Error is null;

    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(Error error) => new(default, error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}
