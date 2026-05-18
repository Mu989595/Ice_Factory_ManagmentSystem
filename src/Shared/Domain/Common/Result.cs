namespace IcePlant.Domain.Common;

/// <summary>
/// Represents the outcome of a domain operation.
/// Use instead of throwing exceptions for expected business failures.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, string error)
    {
        if (isSuccess && error != string.Empty)
            throw new InvalidOperationException("A successful result cannot have an error.");
        if (!isSuccess && error == string.Empty)
            throw new InvalidOperationException("A failed result must have an error message.");

        IsSuccess = isSuccess;
        Error     = error;
    }

    public bool   IsSuccess { get; }
    public bool   IsFailure => !IsSuccess;
    public string Error     { get; }

    public static Result Success()              => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);

    public static Result<T> Success<T>(T value)        => Result<T>.Success(value);
    public static Result<T> Failure<T>(string error)   => Result<T>.Failure(error);
}

/// <summary>
/// Typed result that carries a value on success.
/// </summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value)  : base(true, string.Empty)  => _value = value;
    private Result(string e) : base(false, e)            => _value = default;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value of a failed result.");

    public new static Result<T> Success(T value)        => new(value);
    public new static Result<T> Failure(string error)   => new(error);
}
