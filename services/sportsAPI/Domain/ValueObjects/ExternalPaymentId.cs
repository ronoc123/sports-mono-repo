namespace Domain.ValueObjects
{
    public sealed class ExternalPaymentId
    {
        public string Value { get; }

        private ExternalPaymentId(string value)
        {
            Value = value;
        }

        public static ExternalPaymentId Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("External payment id required.");

            return new ExternalPaymentId(value);
        }
    }
}
