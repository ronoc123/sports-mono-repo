namespace Domain.ValueObjects
{
    public sealed class StripeSessionId
    {
        public string Value { get; }

        private StripeSessionId(string value)
        {
            Value = value;
        }

        public static StripeSessionId Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Stripe Session Id cannot be empty.");

            if (!value.StartsWith("cs_"))
                throw new ArgumentException("Invalid Stripe Session Id format.");

            return new StripeSessionId(value);
        }

        public override string ToString() => Value;
    }
}
