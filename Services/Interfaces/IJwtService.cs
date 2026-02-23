using SecureTaskApi.Entities;

namespace SecureTaskApi.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}