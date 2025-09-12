using Application.Common.Models;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    UserId UserId,
    string Email,
    string UserName
) : IRequest<Result<bool>>;
