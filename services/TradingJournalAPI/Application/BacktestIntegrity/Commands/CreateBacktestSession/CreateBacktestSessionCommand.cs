using Application.Dto;
using Contracts.Contracts;
using MediatR;

namespace Application.BacktestIntegrity.Commands.CreateBacktestSession;

public record CreateBacktestSessionCommand(Guid UserId) : IRequest<ServiceResponse<BacktestSessionDto>>;
