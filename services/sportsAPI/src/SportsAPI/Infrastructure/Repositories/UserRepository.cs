using Domain.Repositories;
using Domain.Users;
using Domain.ValueObjects.ConcreteTypes;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class UserRepository
    : EfRepository<User, UserId>, IUserRepository
{
  private readonly SportsDbAppContext _db;

  public UserRepository(SportsDbAppContext db) : base(db)
  {
    _db = db;
  }

  // Bridge Guid -> UserId; you can also expose FindAsync(UserId) from IRepository<,>
  public Task<User?> GetUserByIdAsync(UserId userId)
      => FindAsync(userId);

  public Task<List<User>> GetAllUsersAsync()
      => _db.Users.AsNoTracking().ToListAsync();

  public Task AddUserAsync(User user)
      => _db.Users.AddAsync(user).AsTask();

  public Task UpdateUserAsync(User user)
  {
    _db.Users.Update(user);
    return Task.CompletedTask;
  }

  public async Task DeleteUserAsync(UserId userId)
  {
    var entity = await FindAsync(userId);
    if (entity is null) return;
    _db.Users.Remove(entity);
  }
}
