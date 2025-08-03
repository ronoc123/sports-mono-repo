using Microsoft.AspNetCore.Mvc;
using sportsAPI.DTO.Code;
using MediatR;
using Application.Codes.Commands.GenerateCode;
using Application.Codes.Commands.RedeemCode;
using sportsAPI.DTO;

namespace sportsAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CodeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CodeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Generate a new code for an organization.
        /// </summary>
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateCode([FromBody] GenerateCodeRequestDto request)
        {
            if (request == null || request.OrganizationId == Guid.Empty)
            {
                return BadRequest(new ServiceResponse<Guid>
                {
                    Success = false,
                    Message = "Organization ID is required."
                });
            }

            var command = new GenerateCodeCommand(request.OrganizationId);
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
                Message = "Code generated successfully.",
                Data = result.Value
            });
        }

        /// <summary>
        /// Redeem an existing code.
        /// </summary>
        [HttpPost("redeem")]
        public async Task<IActionResult> RedeemCode([FromBody] RedeemCodeRequestDto request)
        {
            if (request == null || request.CodeId == Guid.Empty || request.UserId == Guid.Empty)
            {
                return BadRequest(new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Code ID and User ID are required."
                });
            }

            var command = new RedeemCodeCommand(request.CodeId, request.UserId);
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
                Message = "Code redeemed successfully.",
                Data = result.Value
            });
        }

        // Get available codes for a user
        [HttpGet("available/{userId:guid}")]
        public async Task<ActionResult<ServiceResponse<List<RedeemCodeDto>>>> GetAvailableCodes(Guid userId)
        {
            // Mock data - replace with actual repository call
            var codes = new List<RedeemCodeDto>
            {
                new RedeemCodeDto
                {
                    Id = Guid.NewGuid(),
                    Code = "WELCOME100",
                    Type = "votes",
                    Value = 100,
                    Description = "Welcome bonus - 100 free votes",
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new RedeemCodeDto
                {
                    Id = Guid.NewGuid(),
                    Code = "PREMIUM30",
                    Type = "premium",
                    Value = 30,
                    Description = "30 days premium access",
                    ExpiresAt = DateTime.UtcNow.AddDays(60),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                }
            };

            return Ok(new ServiceResponse<List<RedeemCodeDto>>
            {
                Success = true,
                Data = codes,
                Message = "Available codes retrieved successfully."
            });
        }

        // Get user balance
        [HttpGet("balance/{userId:guid}")]
        public async Task<ActionResult<ServiceResponse<UserBalanceDto>>> GetUserBalance(Guid userId)
        {
            // Mock data - replace with actual repository call
            var balance = new UserBalanceDto
            {
                Votes = 250,
                PremiumUntil = DateTime.UtcNow.AddDays(30),
                BonusMultiplier = 1.2m,
                SpecialRewards = new List<string> { "early_access", "exclusive_content" }
            };

            return Ok(new ServiceResponse<UserBalanceDto>
            {
                Success = true,
                Data = balance,
                Message = "User balance retrieved successfully."
            });
        }

        // Redeem code by code string
        [HttpPost("redeem-by-code")]
        public async Task<ActionResult<ServiceResponse<RedeemResponseDto>>> RedeemByCode([FromBody] RedeemRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Code))
            {
                return BadRequest(new ServiceResponse<RedeemResponseDto>
                {
                    Success = false,
                    Message = "Code is required."
                });
            }

            var code = request.Code.ToUpperInvariant();

            // Mock redemption logic - replace with actual business logic
            var mockCodes = new Dictionary<string, (string type, int value, string description)>
            {
                { "WELCOME100", ("votes", 100, "Welcome bonus - 100 free votes") },
                { "PREMIUM30", ("premium", 30, "30 days premium access") },
                { "BONUS25", ("votes", 25, "Daily bonus - 25 votes") }
            };

            if (!mockCodes.ContainsKey(code))
            {
                return BadRequest(new ServiceResponse<RedeemResponseDto>
                {
                    Success = false,
                    Message = "Invalid or expired code. Please check and try again."
                });
            }

            var codeInfo = mockCodes[code];
            var response = new RedeemResponseDto
            {
                Success = true,
                Message = $"Successfully redeemed {codeInfo.value} {codeInfo.type}!",
                Reward = new RewardDto
                {
                    Type = codeInfo.type,
                    Value = codeInfo.value,
                    Description = codeInfo.description
                },
                NewBalance = new UserBalanceDto
                {
                    Votes = codeInfo.type == "votes" ? 250 + codeInfo.value : 250,
                    PremiumUntil = codeInfo.type == "premium" ? DateTime.UtcNow.AddDays(codeInfo.value) : null,
                    BonusMultiplier = 1.2m,
                    SpecialRewards = new List<string> { "early_access", "exclusive_content" }
                }
            };

            return Ok(new ServiceResponse<RedeemResponseDto>
            {
                Success = true,
                Data = response,
                Message = "Code redeemed successfully."
            });
        }

        // Get redemption history for a user
        [HttpGet("history/{userId:guid}")]
        public async Task<ActionResult<ServiceResponse<List<RedemptionHistoryDto>>>> GetRedemptionHistory(Guid userId)
        {
            // Mock data - replace with actual repository call
            var history = new List<RedemptionHistoryDto>
            {
                new RedemptionHistoryDto
                {
                    Id = Guid.NewGuid(),
                    Code = "STARTER50",
                    Type = "votes",
                    Value = 50,
                    Description = "Starter pack - 50 votes",
                    RedeemedAt = DateTime.UtcNow.AddDays(-5),
                    Status = "success",
                    UserId = userId
                },
                new RedemptionHistoryDto
                {
                    Id = Guid.NewGuid(),
                    Code = "BONUS25",
                    Type = "votes",
                    Value = 25,
                    Description = "Daily bonus - 25 votes",
                    RedeemedAt = DateTime.UtcNow.AddDays(-2),
                    Status = "success",
                    UserId = userId
                }
            };

            return Ok(new ServiceResponse<List<RedemptionHistoryDto>>
            {
                Success = true,
                Data = history,
                Message = "Redemption history retrieved successfully."
            });
        }
    }
}
