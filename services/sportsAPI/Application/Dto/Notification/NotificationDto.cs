namespace Application.Dto.Notification
{
    public class NotificationDto
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public Guid OrganizationId { get; set; }

        public string Title { get; set; }
        public string Message { get; set; }

        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
