using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Application.PlayerOptions.Commands.CreatePlayerOption;
using Application.PlayerOptions.Commands.UpdatePlayerOption;
using Application.PlayerOptions.Queries.GetAllPlayerOptions;

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
            return Ok(result);
        }

        // Create a new PlayerOption
        [HttpPost("create")]
        [Authorize(Roles = "Admin,GM,CSP")] // Admin, GM, and CSP can create player options
        public async Task<IActionResult> CreatePlayerOption([FromBody] CreatePlayerOptionCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
            
        }

        // Update a PlayerOption
        [HttpPut("update")]
        [Authorize(Roles = "Admin,GM,CSP")] // Admin, GM, and CSP can update player options
        public async Task<IActionResult> UpdatePlayerOption([FromBody] UpdatePlayerOptionCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        //// Vote on a PlayerOption
        //[HttpPost("{playerOptionId}/vote")]
        //public async Task<IActionResult> VoteOnPlayerOption(Guid playerOptionId)
        //{
        //    // TODO: Implement VoteOnPlayerOptionCommand
        //    return Ok(new ServiceResponse<bool>
        //    {
        //        Success = false,
        //        Message = "Vote functionality not yet implemented - will be added with domain events"
        //    });
        //}

        //// Expire a PlayerOption
        //[HttpPost("{playerOptionId}/expire")]
        //public async Task<IActionResult> ExpirePlayerOption(Guid playerOptionId)
        //{
        //    // TODO: Implement ExpirePlayerOptionCommand
        //    return Ok(new ServiceResponse<bool>
        //    {
        //        Success = false,
        //        Message = "Expire functionality not yet implemented - will be added with domain events"
        //    });
        //}

        //// Get player options for a specific user (with voting status)
        //[HttpGet("user/{userId}")]
        //public async Task<ActionResult<ServiceResponse<List<sportsAPI.DTO.PlayerOption.PlayerOptionDto>>>> GetPlayerOptionsForUser(
        //    Guid userId,
        //    [FromQuery] PlayerOptionFiltersDto filters)
        //{
        //      return null;
        //}

        //// Vote on a player option (enhanced)
        //[HttpPost("vote")]
        //public async Task<ActionResult<ServiceResponse<VoteOnPlayerOptionResponseDto>>> VoteOnPlayerOptionEnhanced(
        //    [FromBody] VoteOnPlayerOptionRequestDto request)
        //{
        //    return null;
        //}

        //// Get player option statistics
        //[HttpGet("stats")]
        //public async Task<ActionResult<ServiceResponse<PlayerOptionStatsDto>>> GetPlayerOptionStats(
        //    [FromQuery] Guid? userId = null,
        //    [FromQuery] Guid? organizationId = null)
        //{
        //      return null;
        //}

        //// Get a specific player option by ID
        //[HttpGet("{playerOptionId}")]
        //public async Task<ActionResult<ServiceResponse<sportsAPI.DTO.PlayerOption.PlayerOptionDto>>> GetPlayerOption(
        //    Guid playerOptionId,
        //    [FromQuery] Guid? userId = null)
        //{
        //      return null;
        //}
    }
}
