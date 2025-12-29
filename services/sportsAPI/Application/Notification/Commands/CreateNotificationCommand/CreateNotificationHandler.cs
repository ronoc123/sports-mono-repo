using Application.Dto.Notification;
using Application.Profiles;
using Contracts.Contracts;
using Domain.Notification;
using Domain.Repositories;
using MediatR;

namespace Application.Notifications.Commands.CreateNotificationCommand
{
    public sealed class CreateNotificationHandler : IRequestHandler<CreateNotificationCommand, ServiceResponse<NotificationDto>>
    {
        private readonly IRepository _repo;

        public CreateNotificationHandler(IRepository repo)
        {
            _repo = repo;
        }
        public async Task<ServiceResponse<NotificationDto>> Handle(CreateNotificationCommand request, CancellationToken ct)
        {
            var notification = Notification.Create(request.UserId, request.OrganizationId, request.Title, request.Message);

            await _repo.AddAsync(notification, ct);
            await _repo.SaveChangesAsync(ct);

            return ServiceResponse.Ok(NotificationDtoMapper.ToNotificationDto(notification), "Notification Created");
        }
    }
}
