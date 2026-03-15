using Application.Dto;
using Contracts.Contracts;
using MediatR;

namespace Application.TradeJournal.Queries.GetTrade;

public record GetTradeQuery(Guid UserId, Guid TradeId) : IRequest<ServiceResponse<TradeDto>>;
