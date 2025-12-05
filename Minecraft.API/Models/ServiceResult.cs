namespace Minecraft.API.Models;

public class ServiceResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ServiceResult<T> SuccessResult(T data, string message = "Operation successful")
    {
        return new ServiceResult<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ServiceResult<T> FailureResult(string message)
    {
        return new ServiceResult<T>
        {
            Success = false,
            Message = message,
            Data = default
        };
    }
}

public class ServiceResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static ServiceResult SuccessResult(string message = "Operation successful")
    {
        return new ServiceResult
        {
            Success = true,
            Message = message
        };
    }

    public static ServiceResult FailureResult(string message)
    {
        return new ServiceResult
        {
            Success = false,
            Message = message
        };
    }
}
