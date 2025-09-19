using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Application.Players.Commands.CreatePlayer;
using Application.Players.Commands.UpdatePlayer;
using Application.Players.Commands.DeletePlayer;
using Application.Players.Queries.GetAllPlayers;
using sportsAPI.DTO;

namespace sportsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] // Temporarily disabled for testing
    public class PlayerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlayerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Get all Players with pagination and filtering
        [HttpGet("all")]
        [AllowAnonymous] // Temporarily allow anonymous access for testing
        public async Task<IActionResult> GetAllPlayers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? leagueId = null,
            [FromQuery] Guid? organizationId = null,
            [FromQuery] string? position = null,
            [FromQuery] int? minAge = null,
            [FromQuery] int? maxAge = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? sortBy = "Name",
            [FromQuery] bool sortDescending = false)
        {
            var query = new GetAllPlayersQuery(
                pageNumber, pageSize, searchTerm, leagueId, organizationId, 
                position, minAge, maxAge, isActive, sortBy, sortDescending);
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // Create a new Player
        [HttpPost("create")]
        [Authorize(Roles = "Admin,GM")]
        public async Task<IActionResult> CreatePlayer([FromBody] CreatePlayerCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        // Update a Player
        [HttpPut("update")]
        [Authorize(Roles = "Admin,GM")]
        public async Task<IActionResult> UpdatePlayer([FromBody] UpdatePlayerCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        // Delete a Player
        [HttpDelete("delete/{playerId}")]
        [Authorize(Roles = "Admin")] // Only Admin can delete players
        public async Task<IActionResult> DeletePlayer(DeletePlayerCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
