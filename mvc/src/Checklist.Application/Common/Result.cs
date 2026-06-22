namespace Checklist.Application.Common;

public class Result
{
    public bool Success { get; }
    public string? Error { get; }

    private Result(bool success, string? error)
    {
        Success = success;
        Error = error;
    }

    public static Result Ok()
    {
        return new Result(true, null);
    }

    public static Result Fail(string error)
    {
        return new Result(false, error);
    }
}

public class Result<T>
{
    public bool Success { get; }
    public string? Error { get; }
    public T? Value { get; }

    private Result(bool success, string? error, T? value)
    {
        Success = success;
        Error = error;
        Value = value;
    }

    public static Result<T> Ok(T? value)
    {
        return new Result<T>(true, null, value);
    }

    public static Result<T> Fail(string error)
    {
        return new Result<T>(false, error, default);
    }
}
