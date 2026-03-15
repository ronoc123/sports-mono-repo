using Application.Dto;
using Contracts.Contracts;
using MediatR;

namespace Application.TradeJournal.Queries.GetUserTrades;

public record GetUserTradesQuery(Guid UserId) : IRequest<ServiceResponse<List<TradeDto>>>;
