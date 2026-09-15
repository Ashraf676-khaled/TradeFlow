// Infrastructure/Identity/UserRepository.cs
namespace TradeFlow.Infrastructure.Identity;

using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Users;
using TradeFlow.Infrastructure.Data;

public class UserRepository : IUserRepository
{
  private readonly AppDbContext _context;

  public UserRepository(AppDbContext context) => _context = context;

  public Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default)
      => _context.Users
          .Include(u => u.RefreshTokens)
          .FirstOrDefaultAsync(u => u.Id == id, ct);

  public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
      => _context.Users.FirstOrDefaultAsync(u => u.Email.Value == email, ct);

  public void Add(User user) => _context.Users.Add(user);
}
