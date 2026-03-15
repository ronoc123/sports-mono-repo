using Application.Dto;
using Contracts.Contracts;
using MediatR;

namespace Application.BacktestIntegrity.Queries.GetBacktestStatus;

public record GetBacktestStatusQuery(Guid UserId) : IRequest<ServiceResponse<BacktestStatusDto>>;
