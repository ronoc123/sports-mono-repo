using Microsoft.AspNetCore.Mvc;
using sportsAPI.DTO.PlayerOption;
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

        // Get player options for a specific user (with voting status)
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ServiceResponse<List<sportsAPI.DTO.PlayerOption.PlayerOptionDto>>>> GetPlayerOptionsForUser(
            Guid userId,
            [FromQuery] PlayerOptionFiltersDto filters)
        {
            // Mock data - replace with actual repository call
            var playerOptions = new List<sportsAPI.DTO.PlayerOption.PlayerOptionDto>
            {
                new sportsAPI.DTO.PlayerOption.PlayerOptionDto
                {
                    Id = Guid.NewGuid(),
                    Title = "Trade Quarterback Decision",
                    Description = "Should we trade our starting quarterback for draft picks?",
                    Category = "trade",
                    Status = "active",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    ExpiresAt = DateTime.UtcNow.AddDays(5),
                    OrganizationId = Guid.NewGuid(),
                    OrganizationName = "Fantasy Football League",
                    PlayerId = Guid.NewGuid(),
                    PlayerName = "Tom Brady",
                    PlayerPosition = "QB",
                    TotalVotes = 45,
                    VotesRequired = 50,
                    HasUserVoted = false,
                    Priority = 5,
                    Tags = new List<string> { "urgent", "quarterback", "trade" },
                    CreatedBy = "GM Mike",
                    Choices = new List<PlayerOptionChoiceDto>
                    {
                        new PlayerOptionChoiceDto
                        {
                            Id = Guid.NewGuid(),
                            Title = "Trade for Draft Picks",
                            Description = "Trade QB for 2 first-round picks",
                            VoteCount = 28,
                            VotePercentage = 62.2m,
                            ImpactDescription = "Rebuild for future seasons"
                        },
                        new PlayerOptionChoiceDto
                        {
                            Id = Guid.NewGuid(),
                            Title = "Keep Current QB",
                            Description = "Maintain current roster",
                            VoteCount = 17,
                            VotePercentage = 37.8m,
                            ImpactDescription = "Compete this season"
                        }
                    }
                },
                new sportsAPI.DTO.PlayerOption.PlayerOptionDto
                {
                    Id = Guid.NewGuid(),
                    Title = "Draft Strategy Focus",
                    Description = "What position should we prioritize in the upcoming draft?",
                    Category = "draft",
                    Status = "active",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    ExpiresAt = DateTime.UtcNow.AddDays(10),
                    OrganizationId = Guid.NewGuid(),
                    OrganizationName = "Basketball Dynasty",
                    TotalVotes = 32,
                    VotesRequired = 40,
                    HasUserVoted = true,
                    UserVoteChoice = "Center",
                    Priority = 3,
                    Tags = new List<string> { "draft", "strategy" },
                    CreatedBy = "Coach Sarah",
                    Choices = new List<PlayerOptionChoiceDto>
                    {
                        new PlayerOptionChoiceDto
                        {
                            Id = Guid.NewGuid(),
                            Title = "Center",
                            Description = "Focus on big man presence",
                            VoteCount = 18,
                            VotePercentage = 56.25m,
                            IsSelected = true,
                            ImpactDescription = "Strengthen interior defense"
                        },
                        new PlayerOptionChoiceDto
                        {
                            Id = Guid.NewGuid(),
                            Title = "Point Guard",
                            Description = "Get a floor general",
                            VoteCount = 14,
                            VotePercentage = 43.75m,
                            ImpactDescription = "Improve ball movement"
                        }
                    }
                }
            };

            return Ok(new ServiceResponse<List<sportsAPI.DTO.PlayerOption.PlayerOptionDto>>
            {
                Success = true,
                Data = playerOptions,
                Message = "Player options retrieved successfully."
            });
        }

        // Vote on a player option (enhanced)
        [HttpPost("vote")]
        public async Task<ActionResult<ServiceResponse<VoteOnPlayerOptionResponseDto>>> VoteOnPlayerOptionEnhanced(
            [FromBody] VoteOnPlayerOptionRequestDto request)
        {
            // Mock voting logic - replace with actual business logic
            var response = new VoteOnPlayerOptionResponseDto
            {
                Success = true,
                Message = "Vote cast successfully!",
                RemainingVotes = 249, // User had 250, now 249
                UpdatedPlayerOption = new sportsAPI.DTO.PlayerOption.PlayerOptionDto
                {
                    Id = request.PlayerOptionId,
                    Title = "Trade Quarterback Decision",
                    Description = "Should we trade our starting quarterback for draft picks?",
                    Category = "trade",
                    Status = "active",
                    TotalVotes = 46, // Increased by 1
                    VotesRequired = 50,
                    HasUserVoted = true,
                    UserVoteChoice = "Trade for Draft Picks",
                    // ... other properties
                }
            };

            return Ok(new ServiceResponse<VoteOnPlayerOptionResponseDto>
            {
                Success = true,
                Data = response,
                Message = "Vote processed successfully."
            });
        }

        // Get player option statistics
        [HttpGet("stats")]
        public async Task<ActionResult<ServiceResponse<PlayerOptionStatsDto>>> GetPlayerOptionStats(
            [FromQuery] Guid? userId = null,
            [FromQuery] Guid? organizationId = null)
        {
            // Mock statistics - replace with actual repository call
            var stats = new PlayerOptionStatsDto
            {
                TotalOptions = 25,
                ActiveOptions = 8,
                CompletedOptions = 15,
                ExpiredOptions = 2,
                UserParticipatedOptions = 12,
                UserVotesUsed = 18,
                UserVotesRemaining = 232,
                CategoryStats = new List<CategoryStatsDto>
                {
                    new CategoryStatsDto
                    {
                        Category = "trade",
                        Count = 8,
                        UserParticipated = 5
                    },
                    new CategoryStatsDto
                    {
                        Category = "draft",
                        Count = 6,
                        UserParticipated = 4
                    },
                    new CategoryStatsDto
                    {
                        Category = "lineup",
                        Count = 7,
                        UserParticipated = 2
                    },
                    new CategoryStatsDto
                    {
                        Category = "strategy",
                        Count = 4,
                        UserParticipated = 1
                    }
                }
            };

            return Ok(new ServiceResponse<PlayerOptionStatsDto>
            {
                Success = true,
                Data = stats,
                Message = "Player option statistics retrieved successfully."
            });
        }

        // Get a specific player option by ID
        [HttpGet("{playerOptionId}")]
        public async Task<ActionResult<ServiceResponse<sportsAPI.DTO.PlayerOption.PlayerOptionDto>>> GetPlayerOption(
            Guid playerOptionId,
            [FromQuery] Guid? userId = null)
        {
            // Mock data - replace with actual repository call
            var playerOption = new sportsAPI.DTO.PlayerOption.PlayerOptionDto
            {
                Id = playerOptionId,
                Title = "Trade Quarterback Decision",
                Description = "Should we trade our starting quarterback for draft picks? This is a critical decision that will impact our team's future for the next 3-5 years.",
                Category = "trade",
                Status = "active",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                ExpiresAt = DateTime.UtcNow.AddDays(5),
                OrganizationId = Guid.NewGuid(),
                OrganizationName = "Fantasy Football League",
                PlayerId = Guid.NewGuid(),
                PlayerName = "Tom Brady",
                PlayerPosition = "QB",
                TotalVotes = 45,
                VotesRequired = 50,
                HasUserVoted = userId.HasValue ? false : false,
                Priority = 5,
                Tags = new List<string> { "urgent", "quarterback", "trade", "future" },
                CreatedBy = "GM Mike",
                Choices = new List<PlayerOptionChoiceDto>
                {
                    new PlayerOptionChoiceDto
                    {
                        Id = Guid.NewGuid(),
                        Title = "Trade for Draft Picks",
                        Description = "Trade QB for 2 first-round picks and a second-round pick",
                        VoteCount = 28,
                        VotePercentage = 62.2m,
                        ImpactDescription = "Rebuild for future seasons, get young talent"
                    },
                    new PlayerOptionChoiceDto
                    {
                        Id = Guid.NewGuid(),
                        Title = "Keep Current QB",
                        Description = "Maintain current roster and compete this season",
                        VoteCount = 17,
                        VotePercentage = 37.8m,
                        ImpactDescription = "Compete this season, risk losing value"
                    }
                }
            };

            return Ok(new ServiceResponse<sportsAPI.DTO.PlayerOption.PlayerOptionDto>
            {
                Success = true,
                Data = playerOption,
                Message = "Player option retrieved successfully."
            });
        }
    }
}
