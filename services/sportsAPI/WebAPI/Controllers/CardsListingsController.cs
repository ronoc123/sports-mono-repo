using Application.Dto.Marketplace;
using Application.Marketplace.Commands.BuyNow;
using Application.Marketplace.Commands.CreateListing;
using Application.Marketplace.Commands.PlaceBid;
using Application.Marketplace.Queries.GetAvailableBalance;
using Application.Marketplace.Queries.GetListingDetail;
using Application.Marketplace.Queries.GetListings;
using Contracts.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace sportsAPI.Controllers;

[Route("api/cards/listings")]
[ApiController]
//[Authorize]
public class CardsListingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CardsListingsController(IMediator mediator) => _mediator = mediator;

    /// <summary>POST /api/cards/listings — List a card for auction.</summary>
    [HttpPost]
    [Authorize(Policy = "UserOnly")]
    public async Task<ActionResult<ServiceResponse<Guid>>> CreateListing(
        [FromBody] CreateListingCommand command)
        => await _mediator.Send(command);

    /// <summary>GET /api/cards/listings?orgId= — Browse active listings (paginated).</summary>
    [HttpGet]
    public async Task<ServiceResponse<List<ListingDto>>> GetListings(
        [FromQuery] Guid orgId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        => await _mediator.Send(new GetListingsQuery(orgId, page, pageSize));

    /// <summary>GET /api/cards/listings/{listingId} — Full listing detail + bid history.</summary>
    [HttpGet("{listingId:guid}")]
    public async Task<ServiceResponse<ListingDetailDto>> GetListingDetail(Guid listingId)
        => await _mediator.Send(new GetListingDetailQuery(listingId));

    /// <summary>POST /api/cards/listings/{listingId}/bids — Place a bid on an active listing.</summary>
    [HttpPost("{listingId:guid}/bids")]
    [Authorize(Policy = "UserOnly")]
    public async Task<ServiceResponse<bool>> PlaceBid(
        Guid listingId,
        [FromBody] PlaceBidRequest body)
        => await _mediator.Send(new PlaceBidCommand(listingId, body.BidderId, body.OrgId, body.Amount));

    /// <summary>POST /api/cards/listings/{listingId}/buy-now — Immediately purchase a card at buy now price.</summary>
    [HttpPost("{listingId:guid}/buy-now")]
    [Authorize(Policy = "UserOnly")]
    public async Task<ServiceResponse<bool>> BuyNow(
        Guid listingId,
        [FromBody] BuyNowRequest body)
        => await _mediator.Send(new BuyNowCommand(listingId, body.BuyerId, body.OrgId));

    /// <summary>GET /api/cards/listings/available-balance?userId=&amp;orgId= — Fan's spendable balance.</summary>
    [HttpGet("available-balance")]
    public async Task<ServiceResponse<long>> GetAvailableBalance(
        [FromQuery] Guid userId,
        [FromQuery] Guid orgId)
        => await _mediator.Send(new GetAvailableBalanceQuery(userId, orgId));
}

// ── Request body DTOs (controller-only, not Application layer) ─────────────────
public record PlaceBidRequest(Guid BidderId, Guid OrgId, long Amount);
public record BuyNowRequest(Guid BuyerId, Guid OrgId);
