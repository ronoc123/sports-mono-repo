using Application.Dto.Notification;
using Domain.Notification;

namespace Application.Profiles
{
    public static class NotificationDtoMapper
    {
        public static NotificationDto ToNotificationDto(Notification notification)
        {
            return new NotificationDto
            {
                NotificationId = notification.Id.Value,
                UserId = notification.UserId.Value,
                OrganizationId = notification.OrganizationId.Value,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt
            };
        }

    }
}
