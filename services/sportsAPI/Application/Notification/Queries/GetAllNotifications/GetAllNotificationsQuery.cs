using Application.Dto.Notification;
using Contracts.Contracts;
using Contracts.Responses;
using MediatR;

namespace Application.Notifications.Queries.GetAllNotifications
{
    public record GetAllNotificationsQuery(Guid userId, int pageNumber, int pageSize) : IRequest<ServiceResponse<PaginatedList<NotificationDto>>>;
}
