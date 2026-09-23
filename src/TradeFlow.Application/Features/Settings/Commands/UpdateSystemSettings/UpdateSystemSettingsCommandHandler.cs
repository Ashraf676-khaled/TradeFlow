namespace TradeFlow.Application.Settings.Commands.UpdateSystemSettings;

using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Settings;

public sealed class UpdateSystemSettingsCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    : IRequestHandler<UpdateSystemSettingsCommand, Result<SystemSettingsDto>>
{
  public async Task<Result<SystemSettingsDto>> Handle(UpdateSystemSettingsCommand request, CancellationToken ct)
  {
    if (currentUser.TenantId is null)
      return Error.Unauthorized("Auth.NoTenant", "Unable to determine the current company.");

    var tenantId = new TenantId(currentUser.TenantId.Value);

    // Normalize the layout casing before persisting (A4 / Thermal).
    var layout = InvoiceLayoutStyles.All
        .First(l => string.Equals(l, request.InvoiceLayoutStyle?.Trim(), StringComparison.OrdinalIgnoreCase));

    var desiredValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
      [SystemSettingKeys.TaxEnabled] = request.TaxEnabled.ToString().ToLowerInvariant(),
      [SystemSettingKeys.TaxPercentage] = request.TaxPercentage.ToString("0.##", CultureInfo.InvariantCulture),
      [SystemSettingKeys.CreditSalesEnabled] = request.CreditSalesEnabled.ToString().ToLowerInvariant(),
      [SystemSettingKeys.LowStockThreshold] = request.LowStockThreshold.ToString(CultureInfo.InvariantCulture),
      [SystemSettingKeys.InvoiceLayoutStyle] = layout,
    };

    var existing = await context.SystemSettings.ToListAsync(ct);
    var byKey = existing.ToDictionary(s => s.Key, StringComparer.OrdinalIgnoreCase);

    foreach (var (key, value) in desiredValues)
    {
      if (byKey.TryGetValue(key, out var setting))
      {
        var changeResult = setting.ChangeValue(value);
        if (changeResult.IsError)
          return changeResult.Errors;
      }
      else
      {
        var createResult = SystemSetting.Create(tenantId, key, value);
        if (createResult.IsError)
          return createResult.Errors;

        context.SystemSettings.Add(createResult.Value);
      }
    }

    // Single SaveChanges → all preferences upserted atomically.
    await context.SaveChangesAsync(ct);

    return SystemSettingsResolver.Resolve(desiredValues);
  }
}