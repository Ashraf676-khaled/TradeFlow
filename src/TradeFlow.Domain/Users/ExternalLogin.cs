namespace TradeFlow.Domain.Users;

using TradeFlow.Domain.Common.Identifiers;

public sealed class ExternalLogin
{
  public Guid Id { get; private set; }
  public UserId UserId { get; private set; }
  public string Provider { get; private set; } = string.Empty;
  public string ProviderSubject { get; private set; } = string.Empty;

  private ExternalLogin() { }

  public ExternalLogin(UserId userId, string provider, string providerSubject)
  {
    Id = Guid.NewGuid();
    UserId = userId;
    Provider = provider;
    ProviderSubject = providerSubject;
  }
}
