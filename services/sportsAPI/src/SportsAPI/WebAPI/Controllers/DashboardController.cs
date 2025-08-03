using Microsoft.AspNetCore.Mvc;
using sportsAPI.DTO.Code;
using sportsAPI.DTO;

namespace sportsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        public DashboardController()
        {
        }

        // Get dashboard statistics
        [HttpGet("stats")]
        public async Task<ActionResult<ServiceResponse<DashboardStatsDto>>> GetDashboardStats()
        {
            // Mock data - replace with actual repository calls
            var stats = new DashboardStatsDto
            {
                ActiveOrganizations = 3,
                TotalUsers = 1247,
                ActivePlayerOptions = 8,
                SystemHealth = 98.5m,
                SystemStatus = "operational",
                QuickActions = new List<QuickActionDto>
                {
                    new QuickActionDto
                    {
                        Label = "Create Player Option",
                        Icon = "add_circle",
                        Route = "/player-options/create",
                        Color = "primary"
                    },
                    new QuickActionDto
                    {
                        Label = "Add New User",
                        Icon = "person_add",
                        Route = "/users/create",
                        Color = "accent"
                    },
                    new QuickActionDto
                    {
                        Label = "View Reports",
                        Icon = "assessment",
                        Route = "/reports",
                        Color = "primary"
                    },
                    new QuickActionDto
                    {
                        Label = "System Settings",
                        Icon = "settings",
                        Route = "/system/settings",
                        Color = "warn"
                    }
                },
                RecentActivities = new List<RecentActivityDto>
                {
                    new RecentActivityDto
                    {
                        Title = "New user registered",
                        Description = "John Doe joined the platform",
                        Timestamp = DateTime.UtcNow.AddMinutes(-30),
                        Type = "success"
                    },
                    new RecentActivityDto
                    {
                        Title = "Player option created",
                        Description = "Trade deadline decision for Team Alpha",
                        Timestamp = DateTime.UtcNow.AddHours(-2),
                        Type = "info"
                    },
                    new RecentActivityDto
                    {
                        Title = "System maintenance",
                        Description = "Scheduled maintenance completed successfully",
                        Timestamp = DateTime.UtcNow.AddHours(-4),
                        Type = "success"
                    },
                    new RecentActivityDto
                    {
                        Title = "High vote activity",
                        Description = "Unusual voting pattern detected",
                        Timestamp = DateTime.UtcNow.AddHours(-6),
                        Type = "warning"
                    }
                }
            };

            return Ok(new ServiceResponse<DashboardStatsDto>
            {
                Success = true,
                Data = stats,
                Message = "Dashboard statistics retrieved successfully."
            });
        }

        // Get user-specific dashboard data
        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<ServiceResponse<object>>> GetUserDashboard(Guid userId)
        {
            // Mock user-specific dashboard data
            var userDashboard = new
            {
                WelcomeMessage = $"Welcome back, User!",
                PersonalStats = new
                {
                    VotesRemaining = 250,
                    OptionsParticipated = 12,
                    OrganizationsJoined = 2,
                    AccountLevel = "gold"
                },
                RecentActivity = new List<object>
                {
                    new
                    {
                        Title = "Voted on player trade",
                        Description = "You voted on the quarterback trade option",
                        Timestamp = DateTime.UtcNow.AddHours(-1),
                        Type = "success"
                    },
                    new
                    {
                        Title = "Joined new organization",
                        Description = "You joined the Fantasy Football League",
                        Timestamp = DateTime.UtcNow.AddDays(-2),
                        Type = "info"
                    }
                },
                QuickActions = new List<object>
                {
                    new
                    {
                        Label = "Vote on Options",
                        Icon = "how_to_vote",
                        Route = "/player-options",
                        Color = "primary"
                    },
                    new
                    {
                        Label = "View Profile",
                        Icon = "person",
                        Route = "/profile",
                        Color = "accent"
                    },
                    new
                    {
                        Label = "Redeem Codes",
                        Icon = "redeem",
                        Route = "/redeem",
                        Color = "primary"
                    }
                }
            };

            return Ok(new ServiceResponse<object>
            {
                Success = true,
                Data = userDashboard,
                Message = "User dashboard data retrieved successfully."
            });
        }
    }
}
