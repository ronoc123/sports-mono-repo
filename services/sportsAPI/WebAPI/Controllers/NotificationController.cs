using Application.Dto.Notification;
using Application.Notifications.Queries.GetAllNotifications;
using Contracts.Contracts;
using Contracts.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("get-all")]
        public async Task<ServiceResponse<PaginatedList<NotificationDto>>> Get([FromBody] GetAllNotificationsQuery query)
        {
            var res = await _mediator.Send(query);
            return res;
        }

    }
}
