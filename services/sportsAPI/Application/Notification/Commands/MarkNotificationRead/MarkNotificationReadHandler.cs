using Application.Dto.Notification;
using Application.Profiles;
using Contracts.Contracts;
using Domain.Notification;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using System.ComponentModel.DataAnnotations;




namespace Application.Notifications.Commands.MarkNotificationRead
{
    public sealed class MarkNotificationReadHandler : IRequestHandler<MarkNotificationReadCommand, ServiceResponse<NotificationDto>>
    {
        private readonly IRepository _repo;

        public MarkNotificationReadHandler(IRepository repository)
        {
            _repo = repository;
        }

        public async Task<ServiceResponse<NotificationDto>> Handle(MarkNotificationReadCommand command, CancellationToken cancellationToken)
        {

            var notification = await _repo.GetByIdAsync<Notification, NotificationId>(
                NotificationId.Of(command.NotificationId)
            );

            if (notification is null)
                throw new ValidationException("Notification Not Found.");

            notification.MarkAsRead();

            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResponse.Ok(NotificationDtoMapper.ToNotificationDto(notification), "Notification marked as read");
        }
    }
}
