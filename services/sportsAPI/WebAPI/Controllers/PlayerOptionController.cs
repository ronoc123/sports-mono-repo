using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Application.PlayerOptions.Commands.CreatePlayerOption;
using Application.PlayerOptions.Commands.UpdatePlayerOption;
using Application.PlayerOptions.Queries.GetAllPlayerOptions;
using Contracts.Contracts;
using Application.Common.Models;

namespace sportsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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

        //Get all PlayerOptions with pagination and filtering
        [HttpGet("GetPlayerOptionsByOrganization")]
        public async Task<IActionResult> GetAllPlayerOptions(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? organizationId = null,
            [FromQuery] Guid? playerId = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] bool? isExpired = null,
            [FromQuery] string? sortBy = "CreatedAt",
            [FromQuery] bool sortDescending = true)
        {
          var query = new GetAllPlayerOptionsQuery(
              pageNumber, pageSize, searchTerm, organizationId, playerId,
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

    }
}
