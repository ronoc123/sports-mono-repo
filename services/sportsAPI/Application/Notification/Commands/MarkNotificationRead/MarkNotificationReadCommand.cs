using Application.Dto.Notification;
using Contracts.Contracts;
using MediatR;

namespace Application.Notifications.Commands.MarkNotificationRead
{
    public record MarkNotificationReadCommand(Guid NotificationId) : IRequest<ServiceResponse<NotificationDto>>;
}
