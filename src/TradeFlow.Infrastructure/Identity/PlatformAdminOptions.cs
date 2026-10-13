namespace TradeFlow.Infrastructure.Identity;

public sealed class PlatformAdminOptions
{
  public Guid? UserId { get; set; }

  public string? Email { get; set; }

  public bool IsAdmin(Guid? userId, string? email = null)
  {
    if (UserId is { } configuredUserId && configuredUserId != Guid.Empty)
      return userId == configuredUserId;

    return
      !string.IsNullOrWhiteSpace(Email) &&
      !string.IsNullOrWhiteSpace(email) &&
      string.Equals(Email.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase);
  }

  public bool IsAdmin(string? email) => IsAdmin(null, email);
}
