using Application.Common.Models;
using MediatR;

namespace Application.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    Guid UserId,
    string Email,
    string UserName
) : IRequest<Result<bool>>;
