using SecureTaskApi.Entities;

namespace SecureTaskApi.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByUsernameAsync(string username);
    Task<Boolean> ExistsByUsernameAsync(string username);
    Task AddAsync(User user);
    Task SaveChangesAsync();
}