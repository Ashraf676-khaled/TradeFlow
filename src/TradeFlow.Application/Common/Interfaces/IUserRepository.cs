// Application/Common/Interfaces/IUserRepository.cs
namespace TradeFlow.Application.Common.Interfaces;

using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Users;

public interface IUserRepository
{
  Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default);
  Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
  void Add(User user);
}
