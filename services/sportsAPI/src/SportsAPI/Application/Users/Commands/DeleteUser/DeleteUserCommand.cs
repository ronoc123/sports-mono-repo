using Application.Common.Models;
using MediatR;

namespace Application.Users.Commands.DeleteUser;

public record DeleteUserCommand(Guid UserId) : IRequest<Result<bool>>;
