using Application.Common.Models;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Users.Queries.GetUser;

public record GetUserQuery(UserId UserId) : IRequest<Result<UserDto>>;
