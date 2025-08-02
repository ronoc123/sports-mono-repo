using Microsoft.AspNetCore.Mvc;
using MediatR;
using Application.Leagues.Commands.CreateLeague;
using Application.Leagues.Commands.UpdateLeague;
using Application.Leagues.Commands.DeleteLeague;
using Application.Leagues.Queries.GetAllLeagues;
using sportsAPI.DTO;

namespace sportsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeagueController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LeagueController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Add a new League
        [HttpPost("add")]
        public async Task<IActionResult> AddLeague([FromBody] CreateLeagueRequestDto request)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new ServiceResponse<Guid>
                {
                    Success = false,
                    Message = "League name cannot be empty."
                });
            }

            var command = new CreateLeagueCommand(request.Name);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(new ServiceResponse<Guid>
                {
                    Success = false,
                    Message = result.Error
                });
            }

            return Ok(new ServiceResponse<Guid>
            {
                Success = true,
                Data = result.Value,
                Message = "League added successfully."
            });
        }

        // Get all Leagues with pagination
        [HttpGet("all")]
        public async Task<IActionResult> GetAllLeagues(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = "Name",
            [FromQuery] bool sortDescending = false)
        {
            var query = new GetAllLeaguesQuery(pageNumber, pageSize, searchTerm, sortBy, sortDescending);
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
                Message = "Leagues retrieved successfully."
            });
        }

        // Update a League
        [HttpPut("update")]
        public async Task<IActionResult> UpdateLeague([FromBody] UpdateLeagueCommand command)
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
                Message = "League updated successfully."
            });
        }

        // Delete a League
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteLeague(Guid id)
        {
            var command = new DeleteLeagueCommand(id);
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
                Message = "League deleted successfully."
            });
        }
    }
}
