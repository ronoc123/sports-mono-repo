using Application.Common.Models;
using Application.Dto.PlayerOption;
using Application.PlayerOptions.Commands.CreatePlayerOption;
using Application.PlayerOptions.Commands.UpdatePlayerOption;
using Application.PlayerOptions.Commands.VotePlayerOption;
using Application.PlayerOptions.Queries.GetAllPlayerOptions;
using Contracts.Contracts;
using Contracts.Responses;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace sportsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlayerOptionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlayerOptionController(IMediator mediator)
        {
            _mediator = mediator;
        }


        // Update a PlayerOption
        [HttpPut("update")]
        public async Task<IActionResult> UpdatePlayerOption([FromBody] UpdatePlayerOptionCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("GetPlayerOptionsByOrganization")]
        public async Task<IActionResult> GetAllPlayerOptions(
            [FromQuery] Guid? organizationId = null,
            [FromQuery] Guid? playerId = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] bool? isExpired = null,
            [FromQuery] string? sortBy = "CreatedAt",
            [FromQuery] bool sortDescending = true)
        {
            var query = new GetAllPlayerOptionsQuery(
                organizationId, playerId, pageNumber, pageSize, searchTerm,
                isActive, isExpired, sortBy, sortDescending);

            var result = await _mediator.Send(query);
            return Ok(result);

        }

        [HttpPost("create")]
        public async Task<ServiceResponse<Guid>> CreatePlayerOption([FromBody] CreatePlayerOptionCommand command)
        {
            var result = await _mediator.Send(command);
            return result;
        }


        [HttpPost("vote")]
        public async Task<IActionResult> vote([FromBody] VotePlayerOptionCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

    }
}
