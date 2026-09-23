namespace TradeFlow.Application.Settings.Queries.GetSystemSettings;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record GetSystemSettingsQuery : IRequest<Result<SystemSettingsDto>>;