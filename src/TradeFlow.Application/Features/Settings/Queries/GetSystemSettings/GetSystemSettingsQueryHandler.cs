namespace TradeFlow.Application.Settings.Queries.GetSystemSettings;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Results;

public sealed class GetSystemSettingsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSystemSettingsQuery, Result<SystemSettingsDto>>
{
  public async Task<Result<SystemSettingsDto>> Handle(GetSystemSettingsQuery request, CancellationToken ct)
  {
    // Tenant query filter applies automatically; missing keys resolve to defaults.
    var settings = await context.SystemSettings.AsNoTracking().ToListAsync(ct);
    return SystemSettingsResolver.Resolve(settings);
  }
}