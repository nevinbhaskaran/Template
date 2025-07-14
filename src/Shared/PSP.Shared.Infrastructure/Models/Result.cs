namespace PSP.Shared.Infrastructure.Models;

/// <summary>
/// Generic result wrapper for API responses
/// </summary>
/// <typeparam name="T">The result data type</typeparam>
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public List<string> Errors { get; private set; } = new();
    public int? StatusCode { get; private set; }

    private Result(bool isSuccess, T? data, string? errorMessage, int? statusCode = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }

    public static Result<T> Success(T data, int statusCode = 200)
        => new(true, data, null, statusCode);

    public static Result<T> Failure(string errorMessage, int statusCode = 400)
        => new(false, default, errorMessage, statusCode);

    public static Result<T> Failure(List<string> errors, int statusCode = 400)
    {
        var result = new Result<T>(false, default, errors.FirstOrDefault(), statusCode);
        result.Errors = errors;
        return result;
    }
}

/// <summary>
/// Non-generic result wrapper
/// </summary>
public class Result
{
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }
    public List<string> Errors { get; private set; } = new();
    public int? StatusCode { get; private set; }

    private Result(bool isSuccess, string? errorMessage, int? statusCode = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }

    public static Result Success(int statusCode = 200)
        => new(true, null, statusCode);

    public static Result Failure(string errorMessage, int statusCode = 400)
        => new(false, errorMessage, statusCode);

    public static Result Failure(List<string> errors, int statusCode = 400)
    {
        var result = new Result(false, errors.FirstOrDefault(), statusCode);
        result.Errors = errors;
        return result;
    }
}
