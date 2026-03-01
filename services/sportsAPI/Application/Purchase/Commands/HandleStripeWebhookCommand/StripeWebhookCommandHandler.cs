using Application.Votes.Commands.EmailRewardRedemption;
using Contracts.Contracts;
using Domain.Enums;
using Domain.Repositories;
using Domain.ValueObjects;
using MediatR;
using PurchaseAggregate = Domain.Purchase.Purchase;
using WebhookEventAggregate = Domain.Purchase.ProcessedWebhookEvent;

namespace Application.Purchase.Commands.HandleStripeWebhookCommand
{
    public class StripeWebhookCommandHandler : IRequestHandler<HandleStripeWebhookCommand, ServiceResponse<Guid>>
    {
        private readonly IRepository _repo;
        private readonly IMediator _mediator;

        public StripeWebhookCommandHandler(IRepository repo, IMediator mediator)
        {
            _repo = repo;
            _mediator = mediator;
        }

        public async Task<ServiceResponse<Guid>> Handle(HandleStripeWebhookCommand command, CancellationToken ct)
        {
            // ── Idempotency check (Story 3.2 AC: duplicate events must not double-credit) ──
            var alreadyProcessed = await _repo.ExistsAsync<WebhookEventAggregate>(
                e => e.StripeEventId == command.EventId, ct);

            if (alreadyProcessed)
                return ServiceResponse.Ok(Guid.Empty, "Event already processed.");

            // ── Resolve purchase ──
            if (string.IsNullOrEmpty(command.PurchaseId) ||
                !Guid.TryParse(command.PurchaseId, out var purchaseId))
                return ServiceResponse.Ok(Guid.Empty, "No purchaseId in event metadata — ignored.");

            var purchase = await _repo.GetByIdAsync<PurchaseAggregate, Guid>(purchaseId, ct);
            if (purchase is null)
                return ServiceResponse.Fail<Guid>($"Purchase {purchaseId} not found.");

            // ── Route by event type ──
            switch (command.EventType)
            {
                case "payment_intent.succeeded":
                    await HandleSucceeded(purchase, command, ct);
                    break;

                case "payment_intent.payment_failed":
                    HandleFailed(purchase);
                    break;

                    // Unrecognised events are recorded but otherwise ignored
            }

            // ── Record idempotency token ──
            var webhookEvent = WebhookEventAggregate.Record(command.EventId, command.EventType);
            await _repo.AddAsync(webhookEvent, ct);
            await _repo.SaveChangesAsync(ct);

            return ServiceResponse.Ok(Guid.Empty, "Webhook processed.");
        }

        private async Task HandleSucceeded(PurchaseAggregate purchase, HandleStripeWebhookCommand command, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(command.PaymentIntentId))
                return;

            // Only transition if still Pending (guards against partial re-execution)
            if (purchase.Status == PurchaseStatus.Pending)
                purchase.MarkPaid(ExternalPaymentId.Create(command.PaymentIntentId));

            // Credit votes via existing mechanism (NFR17)
            if (purchase.FulfillmentStatus == FulfillmentStatus.NotStarted)
            {
                await _mediator.Send(
                    new EmailRewardRedemptionCommand(
                        purchase.UserId.Value,
                        purchase.Item.Quantity,
                        purchase.OrgId.Value),
                    ct);

                purchase.MarkFulfilled();
            }
        }

        private static void HandleFailed(PurchaseAggregate purchase)
        {
            // Only transition if still Pending
            if (purchase.Status == PurchaseStatus.Pending)
                purchase.MarkFailed();
        }
    }
}
