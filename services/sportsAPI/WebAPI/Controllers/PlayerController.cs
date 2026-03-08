using Application.Players.Commands.DeletePlayer;
using Application.Players.Commands.UpdatePlayer;
using Application.Players.Queries.GetAllPlayers;
using Application.Players.Queries.GetRosterByOrganization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace sportsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlayerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlayerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllPlayers([FromQuery] Guid leagueId)
        {
            var request = new GetAllPlayersQuery(leagueId);

            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpGet("roster")]
        public async Task<IActionResult> GetRosterByOrganization([FromQuery] Guid organizationId)
        {
            var result = await _mediator.Send(new GetRosterByOrganizationQuery(organizationId));
            return Ok(result);
        }

        //[HttpPost("create-player")]
        //public async Task<ServiceResponse<PlayerDto>> CreatePlayer([FromBody] CreatePlayerCommand command)
        //{
        //  return await _mediator.Send(command);

        //}

        [HttpPut("update")]
        public async Task<IActionResult> UpdatePlayer([FromBody] UpdatePlayerCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("delete/{playerId}")]
        public async Task<IActionResult> DeletePlayer(DeletePlayerCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
