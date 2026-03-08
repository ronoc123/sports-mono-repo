using Application.Purchase.Commands.CreatePurchaseCommand;
using Application.Store.Queries.GetBundleCatalog;
using Contracts.Contracts;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StoreController : ControllerBase
{
    private readonly IMediator _mediator;

    public StoreController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns active vote bundle tiers available for purchase.
    /// </summary>
    [HttpGet("bundles")]
    public async Task<IActionResult> GetBundles([FromQuery] Guid organizationId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBundleCatalogQuery(organizationId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Initiates a purchase for a vote bundle and returns a Stripe client secret.
    /// </summary>
    [HttpPost("purchases")]
    public async Task<ActionResult<ServiceResponse<CreatePurchaseResponse>>> CreatePurchase(
        [FromBody] CreatePurchaseRequest body,
        CancellationToken ct)
    {
        var command = new CreatePurchaseCommand(
            UserId.Of(body.UserId),
            OrganizationId.Of(body.OrganizationId),
            body.ProductId);

        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }
}

public record CreatePurchaseRequest(Guid UserId, Guid OrganizationId, Guid ProductId);
