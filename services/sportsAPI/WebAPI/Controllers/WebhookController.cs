using Application.Common.Interfaces;
using Application.Purchase.Commands.HandleStripeWebhookCommand;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

/// <summary>
/// Receives and authenticates Stripe webhook events.
/// [AllowAnonymous] — Stripe does not send JWT tokens.
/// Authenticity is verified via HMAC signature (IPaymentProvider.TryParseWebhookEvent).
/// </summary>
[Route("api/store/webhook")]
[ApiController]
[AllowAnonymous]
public class WebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPaymentProvider _payment;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(IMediator mediator, IPaymentProvider payment, ILogger<WebhookController> logger)
    {
        _mediator = mediator;
        _payment = payment;
        _logger = logger;
    }

    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> HandleWebhook(CancellationToken ct)
    {
        // Read raw body before any other processing (NFR8 / Story 3.1 AC)
        string payload;
        using (var reader = new StreamReader(Request.Body))
        {
            payload = await reader.ReadToEndAsync(ct);
        }

        var signatureHeader = Request.Headers["Stripe-Signature"].FirstOrDefault() ?? string.Empty;

        _logger.LogInformation(
            "Stripe webhook received. PayloadLength={Len} HasSignature={HasSig}",
            payload.Length,
            !string.IsNullOrEmpty(signatureHeader));

        if (!_payment.TryParseWebhookEvent(payload, signatureHeader, out var eventData) || eventData is null)
        {
            _logger.LogWarning("Stripe webhook signature validation failed.");
            return BadRequest("Invalid webhook signature.");
        }

        _logger.LogInformation(
            "Stripe webhook verified. EventId={EventId} Type={Type} PurchaseId={PurchaseId}",
            eventData.EventId, eventData.EventType, eventData.PurchaseId);

        await _mediator.Send(
            new HandleStripeWebhookCommand(
                eventData.EventId,
                eventData.EventType,
                eventData.PurchaseId,
                eventData.PaymentIntentId),
            ct);

        return Ok();
    }
}
