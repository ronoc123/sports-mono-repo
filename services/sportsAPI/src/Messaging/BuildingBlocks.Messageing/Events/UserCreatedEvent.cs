
namespace BuildingBlocks.Messageing.Events
{
    public record UserCreatedEvent : IntergrationEvent
    {
        public string EmailAddress { get; set; }

        public string PhoneNumber { get; set; }
    }
}
