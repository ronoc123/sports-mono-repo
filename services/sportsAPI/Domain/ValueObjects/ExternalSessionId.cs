namespace Domain.ValueObjects
{
    public sealed class ExternalSessionId
    {
        public string Value { get; }

        private ExternalSessionId(string value)
        {
            Value = value;
        }

        public static ExternalSessionId Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("External session id is required.");

            if (value.Length > 200)
                throw new ArgumentException("External session id too long.");

            return new ExternalSessionId(value.Trim());
        }

        public override string ToString() => Value;

        public static implicit operator string(ExternalSessionId id)
            => id.Value;
    }
}
