using Application.Cards.Commands.CreateCardPlayer;
using Application.Cards.Commands.PurchasePack;
using Application.Cards.Commands.UpdateCardPlayer;
using Application.Cards.Queries.GetCardPlayers;
using Application.Cards.Queries.GetCollection;
using Application.Dto.Cards;
using Contracts.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace sportsAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CardsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CardsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("players")]
    public async Task<ServiceResponse<List<CardPlayerDto>>> GetCardPlayers([FromQuery] Guid leagueId)
        => await _mediator.Send(new GetCardPlayersQuery(leagueId));

    [HttpPost("players")]
    public async Task<ServiceResponse<CardPlayerDto>> CreateCardPlayer([FromBody] CreateCardPlayerCommand command)
        => await _mediator.Send(command);

    [HttpPut("players/{id:guid}")]
    public async Task<ServiceResponse<CardPlayerDto>> UpdateCardPlayer(
        Guid id,
        [FromBody] UpdateCardPlayerCommand command)
    {
        if (id != command.CardPlayerId)
            return ServiceResponse.Fail<CardPlayerDto>("Route id does not match body CardPlayerId.");

        return await _mediator.Send(command);
    }

    [HttpPost("packs/purchase")]
    public async Task<ServiceResponse<PackPurchaseResultDto>> PurchasePack(
        [FromBody] PurchasePackCommand command)
        => await _mediator.Send(command);

    [HttpGet("collection")]
    public async Task<ServiceResponse<List<UserCardDto>>> GetCollection(
        [FromQuery] Guid userId,
        [FromQuery] Guid leagueId)
        => await _mediator.Send(new GetCollectionQuery(userId, leagueId));
}
