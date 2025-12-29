namespace Domain.Notification
{
    public class Notification : Aggregate<NotificationId>
    {
        public UserId UserId { get; private set; }
        public OrganizationId OrganizationId { get; private set; }

        public string Title { get; private set; }
        public string Message { get; private set; }

        public bool IsRead { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ReadAt { get; private set; }

        internal Notification() { }

        public static Notification Create(UserId userId, OrganizationId organizationId, string title, string message)
        {
            return new Notification
            {
                Id = NotificationId.Of(Guid.NewGuid()),
                UserId = userId,
                OrganizationId = organizationId,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void MarkAsRead()
        {
            if (IsRead) return;
            IsRead = true;
            ReadAt = DateTime.UtcNow;
        }
    }

}
