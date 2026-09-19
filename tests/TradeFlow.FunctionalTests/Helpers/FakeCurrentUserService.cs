namespace TradeFlow.Application.UnitTests.Helpers;

using TradeFlow.Application.Common.Interfaces;

public sealed class FakeCurrentUserService : ICurrentUserService
{
  public Guid? UserId { get; set; }
  public Guid? TenantId { get; set; }
  public string? UserName { get; set; }
  public string? Email { get; set; }
  public bool IsAuthenticated { get; set; } = true;

  public bool IsInRole(string role) => true;
}
