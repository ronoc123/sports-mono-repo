using BuildingBlocks.Exceptions;

namespace Domain.ValueObjects.ConcreteTypes
{
    public record NotificationId
    {
        public Guid Value { get; set; }

        public NotificationId(Guid value)
        {
            Value = value;
        }

        public static NotificationId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value == Guid.Empty)
            {
                throw new DomainException("OrganizationId cannot be empty");
            }

            return new NotificationId(value);
        }
        public bool HasValue => Value != Guid.Empty;
    }
}
