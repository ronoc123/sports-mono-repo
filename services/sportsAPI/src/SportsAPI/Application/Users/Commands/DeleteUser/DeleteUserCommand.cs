using Application.Common.Models;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Users.Commands.DeleteUser;

public record DeleteUserCommand(UserId UserId) : IRequest<Result<bool>>;
