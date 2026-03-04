namespace SecureTaskApi.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Errors { get; init; }

    public static ApiResponse<T> Ok(T? data) =>
        new() { Success = true, Data = data, Errors = null };

    public static ApiResponse<T> Fail(string error) =>
        new() { Success = false, Data = default, Errors = error };
}
