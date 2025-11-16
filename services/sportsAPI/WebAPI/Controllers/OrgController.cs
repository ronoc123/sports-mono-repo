using Application.Common.Models;
using Application.Dto.Organization;
using Application.Organizations.Commands.CreateOrganization;
using Application.Organizations.Commands.DeleteOrganization;
using Application.Organizations.Commands.UpdateOrganization;
using Application.Organizations.Queries.GetAllOrganizations;
using Application.Organizations.Queries.GetOrganizationDetails;
using Application.Themes.Queries.GetTheme;
using Contracts.Contracts;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ServiceResponse<PaginatedList<OrganizationDto>>> GetAllOrganizations(
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
            return result;
            
        }

        [HttpPut("updateOrganization")]
        public async Task<ServiceResponse<bool>> UpdateOrganization([FromBody] UpdateOrganizationCommand command)
        {
            var result = await _mediator.Send(command);
            return result;
        }

        [HttpDelete("deleteOrganization/{organizationId}")]
        public async Task<ServiceResponse<bool>> DeleteOrganization(DeleteOrganizationCommand command)
        {
            var result = await _mediator.Send(command);
            return result;
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
        public async Task<ServiceResponse<OrganizationDetailsDto>> GetOrganizationDetails([FromQuery] Guid organizationId)
        {
            var query = new GetOrganizationDetailsQuery(organizationId);
            return await _mediator.Send(query);
        }



        [HttpPost("addOrganization")]
        public async Task<ServiceResponse<Guid>> AddOrganization([FromBody] CreateOrganizationCommand command)
        {
            var result = await _mediator.Send(command);
            return result;
        }
    }
}
