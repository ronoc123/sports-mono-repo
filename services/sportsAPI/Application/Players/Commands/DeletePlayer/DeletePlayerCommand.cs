using Application.Common.Models;
using MediatR;

namespace Application.Players.Commands.DeletePlayer;

public record DeletePlayerCommand(Guid PlayerId) : IRequest<Result<bool>>;
