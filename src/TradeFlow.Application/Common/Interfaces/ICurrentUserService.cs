namespace TradeFlow.Application.Common.Interfaces;

public interface ICurrentUserService
{
  Guid? UserId { get; }
  Guid? TenantId { get; }
  string? UserName { get; }
  string? Email { get; }
  bool IsAuthenticated { get; }
  bool IsInRole(string role);
}
