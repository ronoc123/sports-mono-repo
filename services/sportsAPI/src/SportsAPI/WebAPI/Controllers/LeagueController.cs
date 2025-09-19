using Microsoft.AspNetCore.Mvc;
using MediatR;
using Application.Leagues.Commands.CreateLeague;
using Application.Leagues.Commands.UpdateLeague;
using Application.Leagues.Commands.DeleteLeague;
using Application.Leagues.Queries.GetAllLeagues;
using Domain.ValueObjects.ConcreteTypes;
using System.ComponentModel.DataAnnotations;

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
        public async Task<IActionResult> AddLeague([FromBody] CreateLeagueCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
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
            return Ok(result);
        }

        // Update a League
        [HttpPut("update")]
        public async Task<IActionResult> UpdateLeague([FromBody] UpdateLeagueCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        // Delete a League
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteLeague(LeagueId id)
        {
            var command = new DeleteLeagueCommand(id);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
