namespace TradeFlow.Infrastructure.Identity;

public sealed class PlatformAdminOptions
{
  public string? Email { get; set; }

  public bool IsAdmin(string email) =>
      !string.IsNullOrWhiteSpace(Email) &&
      string.Equals(Email.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase);
}
