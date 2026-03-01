using Contracts.Contracts;
using MediatR;

namespace Application.Purchase.Commands.HandleStripeWebhookCommand
{
    public sealed record HandleStripeWebhookCommand(
        string EventId,
        string EventType,
        string? PurchaseId,
        string? PaymentIntentId) : IRequest<ServiceResponse<Guid>>;
}
