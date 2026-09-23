namespace TradeFlow.Application.Settings.Commands.UpdateSystemSettings;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record UpdateSystemSettingsCommand(
    bool TaxEnabled,
    decimal TaxPercentage,
    bool CreditSalesEnabled,
    int LowStockThreshold,
    string InvoiceLayoutStyle) : IRequest<Result<SystemSettingsDto>>;