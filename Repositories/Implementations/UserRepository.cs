using Microsoft.EntityFrameworkCore;
using SecureTaskApi.Data;
using SecureTaskApi.Entities;
using SecureTaskApi.Repositories.Interfaces;

namespace SecureTaskApi.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<Boolean> ExistsByUsernameAsync(string username)
    {
        return await _context.Users
            .AnyAsync(u => u.UserName == username);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}