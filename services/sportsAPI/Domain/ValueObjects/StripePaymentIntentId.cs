namespace Domain.ValueObjects
{
    public sealed class StripePaymentIntentId
    {
        public string Value { get; }

        private StripePaymentIntentId(string value)
        {
            Value = value;
        }

        public static StripePaymentIntentId Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Stripe Payment Intent Id cannot be empty.");

            if (!value.StartsWith("pi_"))
                throw new ArgumentException("Invalid Stripe Payment Intent Id format.");

            return new StripePaymentIntentId(value);
        }

        public override string ToString() => Value;
    }

}
