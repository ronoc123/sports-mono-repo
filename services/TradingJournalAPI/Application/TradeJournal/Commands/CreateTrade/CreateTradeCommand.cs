using Application.Dto;
using Contracts.Contracts;
using MediatR;

namespace Application.TradeJournal.Commands.CreateTrade;

public record CreateTradeCommand(Guid UserId, string Symbol, string? Notes)
    : IRequest<ServiceResponse<TradeDto>>;
