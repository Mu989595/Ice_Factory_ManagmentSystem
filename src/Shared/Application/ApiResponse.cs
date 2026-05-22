namespace IcePlant.Application;

/// <summary>
/// Standardized API response wrapper for all endpoints.
/// Ensures consistent response format and better error handling.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; set; }

    public static ApiResponse<T> Ok(T data) =>
        new() { Success = true, Data = data, StatusCode = 200 };

    public static ApiResponse<T> Created(T data) =>
        new() { Success = true, Data = data, StatusCode = 201 };

    public static ApiResponse<T> BadRequest(string error) =>
        new() { Success = false, Error = error, StatusCode = 400 };

    public static ApiResponse<T> Unauthorized(string error = "Unauthorized") =>
        new() { Success = false, Error = error, StatusCode = 401 };

    public static ApiResponse<T> Forbidden(string error = "Access denied") =>
        new() { Success = false, Error = error, StatusCode = 403 };

    public static ApiResponse<T> NotFound(string error = "Resource not found") =>
        new() { Success = false, Error = error, StatusCode = 404 };

    public static ApiResponse<T> ServerError(string error = "An error occurred") =>
        new() { Success = false, Error = error, StatusCode = 500 };
}