using Microsoft.AspNetCore.Mvc;
using MediatR;
using Application.Organizations.Queries.GetOrganizationDetails;
using Application.Organizations.Queries.GetAllOrganizations;
using Application.Organizations.Commands.CreateOrganization;
using Application.Organizations.Commands.UpdateOrganization;
using Application.Organizations.Commands.DeleteOrganization;
using Application.Themes.Queries.GetTheme;
using sportsAPI.DTO;

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
            [FromQuery] bool? isLocked = null,
            [FromQuery] string? sortBy = "Name",
            [FromQuery] bool sortDescending = false)
        {
            var query = new GetAllOrganizationsQuery(
                pageNumber, pageSize, searchTerm, leagueId, sport, isLocked, sortBy, sortDescending);

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
                Message = "Organizations retrieved successfully."
            });
        }

        [HttpPut("updateOrganization")]
        public async Task<ActionResult> UpdateOrganization([FromBody] UpdateOrganizationCommand command)
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
                Message = "Organization updated successfully."
            });
        }

        [HttpDelete("deleteOrganization/{organizationId}")]
        public async Task<ActionResult> DeleteOrganization(Guid organizationId)
        {
            var command = new DeleteOrganizationCommand(organizationId);
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
                Message = "Organization deleted successfully."
            });
        }


        [HttpGet("theme")]
        public async Task<IActionResult> GetTheme([FromQuery] string name)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new ServiceResponse<Application.Themes.Queries.GetTheme.ThemeDto>
                {
                    Success = false,
                    Message = "Theme name cannot be empty."
                });
            }

            // Retrieve theme using MediatR
            var query = new GetThemeQuery(name);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return NotFound(new ServiceResponse<Application.Themes.Queries.GetTheme.ThemeDto>
                {
                    Success = false,
                    Message = result.Error
                });
            }

            // Return successful response
            return Ok(new ServiceResponse<Application.Themes.Queries.GetTheme.ThemeDto>
            {
                Success = true,
                Data = result.Value,
                Message = "Theme retrieved successfully."
            });
        }

        [HttpGet("organizationDetails")]
        public async Task<IActionResult> GetOrganizationDetails([FromQuery] string organizationId)
        {
            if (string.IsNullOrWhiteSpace(organizationId) || !Guid.TryParse(organizationId, out var orgId))
            {
                return BadRequest(new ServiceResponse<OrganizationDetailsDto>
                {
                    Success = false,
                    Message = "Valid Organization ID required."
                });
            }

            // Use MediatR to get organization details
            var query = new GetOrganizationDetailsQuery(orgId);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return NotFound(new ServiceResponse<OrganizationDetailsDto>
                {
                    Success = false,
                    Message = result.Error
                });
            }

            return Ok(new ServiceResponse<OrganizationDetailsDto>
            {
                Success = true,
                Message = "Success",
                Data = result.Value
            });
        }


        // TODO: Implement GetPlayerOptions using Application layer
        [HttpGet("playerOptions/{organizationId}")]
        public async Task<IActionResult> GetPlayerOptions(Guid organizationId)
        {
            if (organizationId == Guid.Empty)
            {
                return BadRequest(new ServiceResponse<object>
                {
                    Success = false,
                    Message = "Organization ID required."
                });
            }

            // This would use a GetPlayerOptionsQuery when implemented
            return Ok(new ServiceResponse<object>
            {
                Success = true,
                Message = "Not implemented yet - use Application layer"
            });
        }


        [HttpPost("addOrganization")]
        public async Task<IActionResult> AddOrganization([FromBody] CreateOrganizationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || request.LeagueId == Guid.Empty)
            {
                return BadRequest(new ServiceResponse<Guid>
                {
                    Success = false,
                    Message = "League Id and name required."
                });
            }

            // Use MediatR to create organization
            var command = new CreateOrganizationCommand(
                request.Name,
                request.LeagueId,
                request.TeamId,
                request.TeamName,
                request.TeamShortName,
                request.FormedYear,
                request.Sport,
                request.Stadium,
                request.Location,
                request.StadiumCapacity,
                request.Website,
                request.Facebook,
                request.Twitter,
                request.Instagram,
                request.Description,
                request.Color1,
                request.Color2,
                request.Color3,
                request.BadgeUrl,
                request.LogoUrl,
                request.Fanart1Url,
                request.Fanart2Url,
                request.Fanart3Url
            );

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
                Message = "Organization created successfully",
                Data = result.Value
            });
        }
    }
}
