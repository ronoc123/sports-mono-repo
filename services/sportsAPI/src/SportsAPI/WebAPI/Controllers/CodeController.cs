using Microsoft.AspNetCore.Mvc;
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
    }
}
