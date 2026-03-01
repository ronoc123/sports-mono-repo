
using Application.Dto.League;
using Application.Leagues.Commands.CreateLeague;
using Application.Leagues.Commands.DeleteLeague;
using Application.Leagues.Commands.UpdateLeague;
using Application.Leagues.Queries.GetAllLeagues;
using Contracts.Contracts;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.AspNetCore.Mvc;


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
        public async Task<ServiceResponse<LeagueId>> AddLeague([FromBody] CreateLeagueCommand command)
            => await _mediator.Send(command);

        // Get all Leagues with pagination
        [HttpGet("all")]
        public async Task<ServiceResponse<List<LeagueDto>>> GetAllLeagues(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = "Name",
            [FromQuery] bool sortDescending = false)
        {
            var query = new GetAllLeaguesQuery(pageNumber, pageSize, searchTerm, sortBy, sortDescending);
            return await _mediator.Send(query);
        }

        // Update a League
        [HttpPut("update")]
        public async Task<ServiceResponse<bool>> UpdateLeague([FromBody] UpdateLeagueCommand command)
        {
            return await _mediator.Send(command);
        }

        // Delete a League
        [HttpDelete("delete")]
        public async Task<ServiceResponse<bool>> DeleteLeague(DeleteLeagueCommand command)
        {
            return await _mediator.Send(command);
        }
    }
}
