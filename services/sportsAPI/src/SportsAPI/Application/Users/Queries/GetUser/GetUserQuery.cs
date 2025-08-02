using Application.Common.Models;
using MediatR;

namespace Application.Users.Queries.GetUser;

public record GetUserQuery(Guid UserId) : IRequest<Result<UserDto>>;
