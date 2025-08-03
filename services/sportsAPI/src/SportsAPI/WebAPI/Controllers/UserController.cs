using Microsoft.AspNetCore.Mvc;
using sportsAPI.DTO.User;
using MediatR;
using Application.Users.Commands.UpdateUser;
using Application.Users.Commands.DeleteUser;
using Application.Users.Queries.GetAllUsers;
using Application.Users.Queries.GetUser;
using sportsAPI.DTO;

namespace sportsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Get all Users with pagination
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = "UserName",
            [FromQuery] bool sortDescending = false)
        {
            var query = new GetAllUsersQuery(pageNumber, pageSize, searchTerm, sortBy, sortDescending);
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
                Message = "Users retrieved successfully."
            });
        }

        // Get a specific User by ID
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(Guid userId)
        {
            var query = new GetUserQuery(userId);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return NotFound(new ServiceResponse<object>
                {
                    Success = false,
                    Message = result.Error
                });
            }

            return Ok(new ServiceResponse<object>
            {
                Success = true,
                Data = result.Value,
                Message = "User retrieved successfully."
            });
        }

        // Update a User
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserCommand command)
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
                Message = "User updated successfully."
            });
        }

        // Delete a User
        [HttpDelete("delete/{userId}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var command = new DeleteUserCommand(userId);
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
                Message = "User deleted successfully."
            });
        }

        // Get User Profile
        [HttpGet("profile/{userId:guid}")]
        public async Task<ActionResult<ServiceResponse<UserProfileDto>>> GetProfile(Guid userId)
        {
            // Mock data for now - replace with actual repository call
            var profile = new UserProfileDto
            {
                Id = userId,
                Email = "john.doe@example.com",
                UserName = "johndoe",
                FirstName = "John",
                LastName = "Doe",
                Phone = "+1 (555) 123-4567",
                DateOfBirth = new DateTime(1990, 5, 15),
                Bio = "Sports enthusiast and team player. Love making strategic decisions for my favorite teams!",
                Avatar = "/assets/default-avatar.png",
                Preferences = new UserPreferencesDto
                {
                    EmailNotifications = true,
                    PushNotifications = false,
                    Theme = "auto",
                    Language = "en",
                    Timezone = "America/New_York",
                    Privacy = new PrivacySettingsDto
                    {
                        ProfileVisibility = "public",
                        ShowEmail = false,
                        ShowPhone = false
                    }
                },
                Stats = new UserStatsDto
                {
                    TotalVotes = 1000,
                    VotesUsed = 750,
                    VotesRemaining = 250,
                    OptionsParticipated = 45,
                    OrganizationsJoined = 3,
                    AccountLevel = "gold",
                    JoinDate = new DateTime(2023, 1, 15)
                },
                CreatedAt = new DateTime(2023, 1, 15, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = DateTime.UtcNow
            };

            return Ok(new ServiceResponse<UserProfileDto>
            {
                Success = true,
                Data = profile,
                Message = "Profile retrieved successfully."
            });
        }

        // Update User Profile
        [HttpPut("profile/{userId:guid}")]
        public async Task<ActionResult<ServiceResponse<UserProfileDto>>> UpdateProfile(Guid userId, UpdateProfileRequestDto request)
        {
            // Validate request
            if (string.IsNullOrEmpty(request.FirstName) || string.IsNullOrEmpty(request.LastName))
            {
                return BadRequest(new ServiceResponse<UserProfileDto>
                {
                    Success = false,
                    Message = "First name and last name are required"
                });
            }

            // Mock update - replace with actual repository call
            var updatedProfile = new UserProfileDto
            {
                Id = userId,
                Email = "john.doe@example.com", // Keep existing email
                UserName = request.UserName ?? "johndoe",
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                DateOfBirth = request.DateOfBirth,
                Bio = request.Bio,
                Avatar = "/assets/default-avatar.png", // Keep existing avatar
                UpdatedAt = DateTime.UtcNow
            };

            return Ok(new ServiceResponse<UserProfileDto>
            {
                Success = true,
                Data = updatedProfile,
                Message = "Profile updated successfully."
            });
        }

        // Update User Preferences
        [HttpPut("profile/{userId:guid}/preferences")]
        public async Task<ActionResult<ServiceResponse<UserPreferencesDto>>> UpdatePreferences(Guid userId, UpdatePreferencesRequestDto request)
        {
            // Mock update - replace with actual repository call
            var updatedPreferences = new UserPreferencesDto
            {
                EmailNotifications = request.EmailNotifications ?? true,
                PushNotifications = request.PushNotifications ?? false,
                Theme = request.Theme ?? "auto",
                Language = request.Language ?? "en",
                Timezone = request.Timezone ?? "UTC",
                Privacy = request.Privacy ?? new PrivacySettingsDto()
            };

            return Ok(new ServiceResponse<UserPreferencesDto>
            {
                Success = true,
                Data = updatedPreferences,
                Message = "Preferences updated successfully."
            });
        }

        // Upload Avatar
        [HttpPost("profile/{userId:guid}/avatar")]
        public async Task<ActionResult<ServiceResponse<string>>> UploadAvatar(Guid userId, [FromForm] UploadAvatarRequestDto request)
        {
            if (request.Avatar == null || request.Avatar.Length == 0)
            {
                return BadRequest(new ServiceResponse<string>
                {
                    Success = false,
                    Message = "No file uploaded"
                });
            }

            // Mock file upload - replace with actual file storage service
            var fileName = $"avatar_{userId}_{DateTime.UtcNow.Ticks}.jpg";
            var avatarUrl = $"/uploads/avatars/{fileName}";

            return Ok(new ServiceResponse<string>
            {
                Success = true,
                Data = avatarUrl,
                Message = "Avatar uploaded successfully."
            });
        }

        // Get users for management (admin endpoint)
        [HttpGet("management")]
        public async Task<ActionResult<ServiceResponse<List<UserManagementDto>>>> GetUsersForManagement(
            [FromQuery] UserSearchFiltersDto filters)
        {
            // Mock data - replace with actual repository call
            var users = new List<UserManagementDto>
            {
                new UserManagementDto
                {
                    Id = Guid.NewGuid(),
                    Email = "john.doe@example.com",
                    UserName = "johndoe",
                    FirstName = "John",
                    LastName = "Doe",
                    Phone = "+1 (555) 123-4567",
                    AccountLevel = "gold",
                    Status = "active",
                    CreatedAt = DateTime.UtcNow.AddMonths(-6),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2),
                    LastLoginAt = DateTime.UtcNow.AddHours(-3),
                    ActivityStats = new UserActivityStatsDto
                    {
                        TotalVotes = 1000,
                        VotesUsed = 750,
                        VotesRemaining = 250,
                        OptionsParticipated = 45,
                        OrganizationsJoined = 3,
                        CodesRedeemed = 8,
                        LastActivity = DateTime.UtcNow.AddHours(-1),
                        LoginCount = 127,
                        EngagementScore = 85.5
                    },
                    Organizations = new List<UserOrganizationDto>
                    {
                        new UserOrganizationDto
                        {
                            OrganizationId = Guid.NewGuid(),
                            OrganizationName = "Fantasy Football League",
                            Role = "member",
                            JoinedAt = DateTime.UtcNow.AddMonths(-4),
                            IsActive = true
                        }
                    },
                    Roles = new List<string> { "User" },
                    Permissions = new List<string> { "vote", "redeem_codes" }
                },
                new UserManagementDto
                {
                    Id = Guid.NewGuid(),
                    Email = "jane.smith@example.com",
                    UserName = "janesmith",
                    FirstName = "Jane",
                    LastName = "Smith",
                    AccountLevel = "silver",
                    Status = "active",
                    CreatedAt = DateTime.UtcNow.AddMonths(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    LastLoginAt = DateTime.UtcNow.AddDays(-1),
                    ActivityStats = new UserActivityStatsDto
                    {
                        TotalVotes = 500,
                        VotesUsed = 320,
                        VotesRemaining = 180,
                        OptionsParticipated = 22,
                        OrganizationsJoined = 2,
                        CodesRedeemed = 4,
                        LastActivity = DateTime.UtcNow.AddDays(-1),
                        LoginCount = 89,
                        EngagementScore = 72.3
                    },
                    Organizations = new List<UserOrganizationDto>
                    {
                        new UserOrganizationDto
                        {
                            OrganizationId = Guid.NewGuid(),
                            OrganizationName = "Basketball Dynasty",
                            Role = "admin",
                            JoinedAt = DateTime.UtcNow.AddMonths(-2),
                            IsActive = true
                        }
                    },
                    Roles = new List<string> { "User", "Admin" },
                    Permissions = new List<string> { "vote", "redeem_codes", "manage_organization" }
                }
            };

            return Ok(new ServiceResponse<List<UserManagementDto>>
            {
                Success = true,
                Data = users,
                Message = "Users retrieved successfully."
            });
        }

        // Get user statistics overview
        [HttpGet("stats")]
        public async Task<ActionResult<ServiceResponse<UserStatsOverviewDto>>> GetUserStatsOverview()
        {
            // Mock data - replace with actual repository call
            var stats = new UserStatsOverviewDto
            {
                TotalUsers = 1247,
                ActiveUsers = 1089,
                SuspendedUsers = 12,
                InactiveUsers = 146,
                NewUsersThisMonth = 89,
                NewUsersThisWeek = 23,
                AverageEngagementScore = 76.8,
                LevelStats = new List<UserLevelStatsDto>
                {
                    new UserLevelStatsDto { Level = "bronze", Count = 623, Percentage = 49.9 },
                    new UserLevelStatsDto { Level = "silver", Count = 374, Percentage = 30.0 },
                    new UserLevelStatsDto { Level = "gold", Count = 187, Percentage = 15.0 },
                    new UserLevelStatsDto { Level = "platinum", Count = 63, Percentage = 5.1 }
                },
                ActivityTrends = new List<UserActivityTrendDto>
                {
                    new UserActivityTrendDto
                    {
                        Date = DateTime.UtcNow.AddDays(-6),
                        ActiveUsers = 456,
                        NewRegistrations = 12,
                        VotesCast = 234,
                        CodesRedeemed = 45
                    },
                    new UserActivityTrendDto
                    {
                        Date = DateTime.UtcNow.AddDays(-5),
                        ActiveUsers = 523,
                        NewRegistrations = 18,
                        VotesCast = 289,
                        CodesRedeemed = 52
                    }
                }
            };

            return Ok(new ServiceResponse<UserStatsOverviewDto>
            {
                Success = true,
                Data = stats,
                Message = "User statistics retrieved successfully."
            });
        }

        // Create new user (admin endpoint)
        [HttpPost("create")]
        public async Task<ActionResult<ServiceResponse<UserManagementDto>>> CreateUser([FromBody] CreateUserRequestDto request)
        {
            // Validate request
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new ServiceResponse<UserManagementDto>
                {
                    Success = false,
                    Message = "Email and password are required"
                });
            }

            // Mock creation - replace with actual business logic
            var newUser = new UserManagementDto
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                UserName = request.UserName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                DateOfBirth = request.DateOfBirth,
                Bio = request.Bio,
                AccountLevel = "bronze",
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Roles = request.Roles.Any() ? request.Roles : new List<string> { "User" },
                ActivityStats = new UserActivityStatsDto
                {
                    TotalVotes = 100, // Starting votes
                    VotesRemaining = 100,
                    EngagementScore = 0
                }
            };

            return CreatedAtAction(
                nameof(GetUser),
                new { userId = newUser.Id },
                new ServiceResponse<UserManagementDto>
                {
                    Success = true,
                    Data = newUser,
                    Message = "User created successfully."
                });
        }

        // Update user status (admin endpoint)
        [HttpPut("status")]
        public async Task<ActionResult<ServiceResponse<bool>>> UpdateUserStatus([FromBody] UpdateUserStatusRequestDto request)
        {
            // Mock update - replace with actual business logic
            return Ok(new ServiceResponse<bool>
            {
                Success = true,
                Data = true,
                Message = $"User status updated to {request.Status} successfully."
            });
        }

        // Assign user role (admin endpoint)
        [HttpPost("assign-role")]
        public async Task<ActionResult<ServiceResponse<bool>>> AssignUserRole([FromBody] AssignUserRoleRequestDto request)
        {
            // Mock assignment - replace with actual business logic
            return Ok(new ServiceResponse<bool>
            {
                Success = true,
                Data = true,
                Message = $"Role {request.Role} assigned to user successfully."
            });
        }

        // Bulk user actions (admin endpoint)
        [HttpPost("bulk-action")]
        public async Task<ActionResult<ServiceResponse<BulkUserActionResponseDto>>> BulkUserAction([FromBody] BulkUserActionRequestDto request)
        {
            // Mock bulk action - replace with actual business logic
            var response = new BulkUserActionResponseDto
            {
                TotalRequested = request.UserIds.Count,
                Successful = request.UserIds.Count - 1, // Mock one failure
                Failed = 1,
                Results = request.UserIds.Select((userId, index) => new BulkActionResultDto
                {
                    UserId = userId,
                    Success = index != 0, // Mock first one failing
                    Error = index == 0 ? "User not found" : null
                }).ToList()
            };

            return Ok(new ServiceResponse<BulkUserActionResponseDto>
            {
                Success = true,
                Data = response,
                Message = $"Bulk action {request.Action} completed."
            });
        }
    }
}
