using System.Text.Json;

namespace SecureTaskWeb.Helpers;

/// <summary>
/// Helper to decode JWT tokens and extract claims
/// </summary>
public static class JwtHelper
{
    /// <summary>
    /// Decode JWT token and get expiry time (exp claim)
    /// </summary>
    public static DateTime? GetTokenExpiry(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return null;

            // Decode payload (second part)
            var payload = parts[1];
            // Add padding if needed
            int padding = 4 - (payload.Length % 4);
            if (padding != 4) payload += new string('=', padding);

            var decodedBytes = Convert.FromBase64String(payload);
            var json = System.Text.Encoding.UTF8.GetString(decodedBytes);

            using (var doc = JsonDocument.Parse(json))
            {
                if (doc.RootElement.TryGetProperty("exp", out var expElement) &&
                    expElement.TryGetInt64(out long expValue))
                {
                    // exp is in Unix timestamp (seconds since epoch)
                    return UnixTimeStampToDateTime(expValue);
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Check if token is expired
    /// </summary>
    public static bool IsTokenExpired(string token)
    {
        var expiry = GetTokenExpiry(token);
        if (expiry == null) return true;
        return DateTime.UtcNow >= expiry.Value;
    }

    /// <summary>
    /// Convert Unix timestamp to DateTime
    /// </summary>
    private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
        return dateTime;
    }
}
