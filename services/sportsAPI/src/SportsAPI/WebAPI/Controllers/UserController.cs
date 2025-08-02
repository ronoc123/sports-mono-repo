using Microsoft.AspNetCore.Mvc;
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
    }
}
