using Application.Dto;
using Contracts.Contracts;
using MediatR;

namespace Application.BacktestIntegrity.Commands.SubmitBacktestMetrics;

public record SubmitBacktestMetricsCommand(
    Guid UserId,
    int TotalTrades,
    decimal WinRate,
    decimal AvgRR
) : IRequest<ServiceResponse<BacktestStatusDto>>;
