using Domain.ValueObjects;

namespace Application.Common.Interfaces
{
    public interface IPaymentProvider
    {
        Task<CreatePaymentSessionResult> CreatePaymentIntentAsync(
            Guid purchaseId,
            string description,
            Money price,
            CancellationToken ct = default);

        /// <summary>
        /// Verifies the webhook signature and parses the event metadata from the raw payload.
        /// Returns false if the signature is invalid or cannot be parsed.
        /// All Stripe-SDK parsing is isolated here — Application layer never imports Stripe.
        /// </summary>
        bool TryParseWebhookEvent(
            string payload,
            string signatureHeader,
            out WebhookEventData? eventData);
    }

    public record CreatePaymentSessionResult(string PaymentIntentId, string ClientSecret);

    public sealed record WebhookEventData(
        string EventId,
        string EventType,
        string? PurchaseId,        // from PaymentIntent.Metadata["purchaseId"]
        string? PaymentIntentId);  // data.object.id for payment_intent.* events
}
