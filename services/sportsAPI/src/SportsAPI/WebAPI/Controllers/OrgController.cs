using Microsoft.AspNetCore.Mvc;
using MediatR;
using Application.Organizations.Queries.GetOrganizationDetails;
using Application.Organizations.Queries.GetAllOrganizations;
using Application.Organizations.Commands.CreateOrganization;
using Application.Organizations.Commands.UpdateOrganization;
using Application.Organizations.Commands.DeleteOrganization;
using Application.Themes.Queries.GetTheme;
using Domain.ValueObjects.ConcreteTypes;

namespace sportsAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrgController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrgController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("GetAllOrganization")]
        public async Task<ActionResult> GetAllOrganizations(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? leagueId = null,
            [FromQuery] string? sport = null,
            [FromQuery] string? sortBy = "Name",
            [FromQuery] bool sortDescending = false)
        {
            var query = new GetAllOrganizationsQuery(
                pageNumber, pageSize, searchTerm, leagueId, sport, sortBy, sortDescending);

            var result = await _mediator.Send(query);
            return Ok(result);
            
        }

        [HttpPut("updateOrganization")]
        public async Task<ActionResult> UpdateOrganization([FromBody] UpdateOrganizationCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("deleteOrganization/{organizationId}")]
        public async Task<ActionResult> DeleteOrganization(DeleteOrganizationCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }


        [HttpGet("theme")]
        public async Task<IActionResult> GetTheme([FromQuery] string name)
        {

            // Retrieve theme using MediatR
            var query = new GetThemeQuery(name);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("organizationDetails")]
        public async Task<IActionResult> GetOrganizationDetails([FromQuery] OrganizationId organizationId)
        {
            var query = new GetOrganizationDetailsQuery(organizationId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }


        // TODO: Implement GetPlayerOptions using Application layer
        [HttpGet("playerOptions/{organizationId}")]
        public async Task<IActionResult> GetPlayerOptions(Guid organizationId)
        {
           return Ok();
        }


        [HttpPost("addOrganization")]
        public async Task<IActionResult> AddOrganization([FromBody] CreateOrganizationCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
