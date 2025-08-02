using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Application.PlayerOptions.Commands.CreatePlayerOption;
using Application.PlayerOptions.Commands.UpdatePlayerOption;
using Application.PlayerOptions.Queries.GetAllPlayerOptions;
using sportsAPI.DTO;

namespace sportsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication for all endpoints
    public class PlayerOptionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlayerOptionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Get all PlayerOptions with pagination and filtering
        [HttpGet("all")]
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

            if (!result.IsSuccess)
            {
                return BadRequest(new ServiceResponse<object>
                {
                    Success = false,
                    Message = result.Error
                });
            }

            return Ok(new ServiceResponse<object>
            {
                Success = true,
                Data = result.Value,
                Message = "Player options retrieved successfully."
            });
        }

        // Create a new PlayerOption
        [HttpPost("create")]
        [Authorize(Roles = "Admin,GM,CSP")] // Admin, GM, and CSP can create player options
        public async Task<IActionResult> CreatePlayerOption([FromBody] CreatePlayerOptionCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(new ServiceResponse<Guid>
                {
                    Success = false,
                    Message = result.Error
                });
            }

            return CreatedAtAction(
                nameof(GetAllPlayerOptions), 
                new { }, 
                new ServiceResponse<Guid>
                {
                    Success = true,
                    Data = result.Value,
                    Message = "Player option created successfully."
                });
        }

        // Update a PlayerOption
        [HttpPut("update")]
        [Authorize(Roles = "Admin,GM,CSP")] // Admin, GM, and CSP can update player options
        public async Task<IActionResult> UpdatePlayerOption([FromBody] UpdatePlayerOptionCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(new ServiceResponse<bool>
                {
                    Success = false,
                    Message = result.Error
                });
            }

            return Ok(new ServiceResponse<bool>
            {
                Success = true,
                Data = result.Value,
                Message = "Player option updated successfully."
            });
        }

        // Vote on a PlayerOption
        [HttpPost("{playerOptionId}/vote")]
        public async Task<IActionResult> VoteOnPlayerOption(Guid playerOptionId)
        {
            // TODO: Implement VoteOnPlayerOptionCommand
            return Ok(new ServiceResponse<bool>
            {
                Success = false,
                Message = "Vote functionality not yet implemented - will be added with domain events"
            });
        }

        // Expire a PlayerOption
        [HttpPost("{playerOptionId}/expire")]
        public async Task<IActionResult> ExpirePlayerOption(Guid playerOptionId)
        {
            // TODO: Implement ExpirePlayerOptionCommand
            return Ok(new ServiceResponse<bool>
            {
                Success = false,
                Message = "Expire functionality not yet implemented - will be added with domain events"
            });
        }
    }
}
