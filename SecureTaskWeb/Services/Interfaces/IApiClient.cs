namespace SecureTaskWeb.Services.Interfaces;

/// <summary>
/// Generic HTTP client interface for API communication
/// </summary>
public interface IApiClient
{
    /// <summary>
    /// Makes a POST request to the API
    /// </summary>
    Task<ApiResult<T>> PostAsync<T>(string endpoint, object? payload = null);

    /// <summary>
    /// Makes a GET request to the API
    /// </summary>
    Task<ApiResult<T>> GetAsync<T>(string endpoint);

    /// <summary>
    /// Makes a PUT request to the API
    /// </summary>
    Task<ApiResult<T>> PutAsync<T>(string endpoint, object? payload = null);

    /// <summary>
    /// Makes a DELETE request to the API
    /// </summary>
    Task<ApiResult<T>> DeleteAsync<T>(string endpoint);
}

/// <summary>
/// Result wrapper for API responses
/// </summary>
public class ApiResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public int StatusCode { get; set; }
}
