using Domain.Repositories;
using Domain.Users;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SportsDbAppContext _db;

    public UserRepository(SportsDbAppContext db)
    {
        _db = db;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _db.Users
            .FirstOrDefaultAsync(u => u.Id.Value == userId);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _db.Users.ToListAsync();
    }

    public async Task AddUserAsync(User user)
    {
        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id.Value == userId);
        
        if (user != null)
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
        }
    }
}
