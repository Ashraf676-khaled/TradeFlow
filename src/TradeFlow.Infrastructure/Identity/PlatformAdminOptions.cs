namespace TradeFlow.Infrastructure.Identity;

public sealed class PlatformAdminOptions
{
  public string[] UserIds { get; set; } = [];

  public bool IsAdmin(Guid userId) =>
      UserIds.Any(configuredUserId =>
          Guid.TryParse(configuredUserId, out var parsedUserId) && parsedUserId == userId);
}
