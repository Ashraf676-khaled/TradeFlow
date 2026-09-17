namespace TradeFlow.Domain.Tenants;

using TradeFlow.Domain.Common.Results;

public static class TenantErrors
{
  public static readonly Error NameRequired = Error.Validation(
      "Tenant.NameRequired", "Company name is required.");

  public static readonly Error NameTooLong = Error.Validation(
      "Tenant.NameTooLong", "Company name cannot exceed 200 characters.");

  public static readonly Error AlreadyInactive = Error.Conflict(
      "Tenant.AlreadyInactive", "Tenant is already inactive.");

  public static readonly Error NotFound = Error.NotFound(
      "Tenant.NotFound", "Tenant not found.");
}
