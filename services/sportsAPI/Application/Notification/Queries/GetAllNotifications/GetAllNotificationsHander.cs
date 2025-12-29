using Application.Dto.Notification;
using Application.Profiles;
using Contracts.Contracts;
using Contracts.Responses;
using Domain.Notification;
using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;




namespace Application.Notifications.Queries.GetAllNotifications
{
    public sealed class GetAllNotificationsHander : IRequestHandler<GetAllNotificationsQuery, ServiceResponse<PaginatedList<NotificationDto>>>
    {
        private readonly IRepository _repository;

        public GetAllNotificationsHander(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<ServiceResponse<PaginatedList<NotificationDto>>> Handle(GetAllNotificationsQuery query, CancellationToken cancellationToken)
        {

            var notifications = await _repository
                .Query<Notification>()
                .ToListAsync(cancellationToken);

            var dtoList = notifications
                .Select(NotificationDtoMapper.ToNotificationDto)
                .ToList();

            var page = PaginatedListFactory.Create(
              dtoList, query.pageNumber, query.pageSize);

            return ServiceResponse.Ok(page);
        }
    }
}
