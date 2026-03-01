using Application.Common.Interfaces;
using Domain.ValueObjects;
using Microsoft.Extensions.Options;
using Stripe;

namespace Infrastructure.Store
{
    public sealed class StripePaymentProvider : IPaymentProvider
    {
        private readonly StripeOptions _opts;

        public StripePaymentProvider(IOptions<StripeOptions> opts)
        {
            _opts = opts.Value;
            StripeConfiguration.ApiKey = _opts.SecretKey;
        }

        public async Task<CreatePaymentSessionResult> CreatePaymentIntentAsync(
            Guid purchaseId,
            string description,
            Money price,
            CancellationToken ct = default)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(price.Amount * 100),
                Currency = price.Currency.ToLowerInvariant(),
                Description = description,
                Metadata = new Dictionary<string, string>
                {
                    ["purchaseId"] = purchaseId.ToString()
                },
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                }
            };

            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options, cancellationToken: ct);

            return new CreatePaymentSessionResult(intent.Id, intent.ClientSecret);
        }

        public bool TryParseWebhookEvent(
            string payload,
            string signatureHeader,
            out WebhookEventData? eventData)
        {
            eventData = null;

            if (string.IsNullOrEmpty(signatureHeader))
                return false;

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    payload,
                    signatureHeader,
                    _opts.WebhookSecret,
                    throwOnApiVersionMismatch: false);

                string? purchaseId = null;
                string? paymentIntentId = null;

                if (stripeEvent.Data.Object is PaymentIntent paymentIntent)
                {
                    paymentIntentId = paymentIntent.Id;
                    paymentIntent.Metadata.TryGetValue("purchaseId", out purchaseId);
                }

                eventData = new WebhookEventData(
                    stripeEvent.Id,
                    stripeEvent.Type,
                    purchaseId,
                    paymentIntentId);

                return true;
            }
            catch (StripeException)
            {
                return false;
            }
        }
    }
}
