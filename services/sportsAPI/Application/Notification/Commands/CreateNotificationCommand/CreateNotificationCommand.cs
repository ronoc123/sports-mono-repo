using Application.Dto.Notification;
using Contracts.Contracts;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Notifications.Commands.CreateNotificationCommand
{
    public sealed record CreateNotificationCommand(UserId UserId, OrganizationId OrganizationId, RewardItemId RewardItemId, string Title, string Message) : IRequest<ServiceResponse<NotificationDto>>;

}
