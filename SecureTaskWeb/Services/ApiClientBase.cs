using System.Net.Http.Json;
using SecureTaskWeb.Services.Interfaces;

namespace SecureTaskWeb.Services;

/// <summary>
/// Base API client for making HTTP requests to the backend API
/// </summary>
public class ApiClientBase : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClientBase> _logger;

    public ApiClientBase(HttpClient httpClient, ILogger<ApiClientBase> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ApiResult<T>> PostAsync<T>(string endpoint, object? payload = null)
    {
        try
        {
            _logger.LogInformation("POST request to {Endpoint}", endpoint);

            var response = await _httpClient.PostAsJsonAsync(endpoint, payload);

            return await HandleResponse<T>(response, endpoint, "POST");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making POST request to {Endpoint}", endpoint);
            return new ApiResult<T>
            {
                Success = false,
                Error = $"Request failed: {ex.Message}",
                StatusCode = 0
            };
        }
    }

    public async Task<ApiResult<T>> GetAsync<T>(string endpoint)
    {
        try
        {
            _logger.LogInformation("GET request to {Endpoint}", endpoint);

            var response = await _httpClient.GetAsync(endpoint);

            return await HandleResponse<T>(response, endpoint, "GET");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making GET request to {Endpoint}", endpoint);
            return new ApiResult<T>
            {
                Success = false,
                Error = $"Request failed: {ex.Message}",
                StatusCode = 0
            };
        }
    }

    public async Task<ApiResult<T>> PutAsync<T>(string endpoint, object? payload = null)
    {
        try
        {
            _logger.LogInformation("PUT request to {Endpoint}", endpoint);

            var response = await _httpClient.PutAsJsonAsync(endpoint, payload);

            return await HandleResponse<T>(response, endpoint, "PUT");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making PUT request to {Endpoint}", endpoint);
            return new ApiResult<T>
            {
                Success = false,
                Error = $"Request failed: {ex.Message}",
                StatusCode = 0
            };
        }
    }

    public async Task<ApiResult<T>> DeleteAsync<T>(string endpoint)
    {
        try
        {
            _logger.LogInformation("DELETE request to {Endpoint}", endpoint);

            var response = await _httpClient.DeleteAsync(endpoint);

            return await HandleResponse<T>(response, endpoint, "DELETE");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making DELETE request to {Endpoint}", endpoint);
            return new ApiResult<T>
            {
                Success = false,
                Error = $"Request failed: {ex.Message}",
                StatusCode = 0
            };
        }
    }

    private async Task<ApiResult<T>> HandleResponse<T>(HttpResponseMessage response, string endpoint, string method)
    {
        try
        {
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<T>();
                _logger.LogInformation("{Method} {Endpoint} succeeded", method, endpoint);

                return new ApiResult<T>
                {
                    Success = true,
                    Data = data,
                    StatusCode = (int)response.StatusCode
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("{Method} {Endpoint} failed with status {StatusCode}: {Error}",
                    method, endpoint, response.StatusCode, errorContent);

                return new ApiResult<T>
                {
                    Success = false,
                    Error = errorContent,
                    StatusCode = (int)response.StatusCode
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling response from {Method} {Endpoint}", method, endpoint);
            throw;
        }
    }
}
