using Application.Common.Models;
using MediatR;

namespace Application.Players.Commands.UpdatePlayer;

public record UpdatePlayerCommand(
    Guid PlayerId,
    string Name,
    string Position,
    string ImageUrl,
    int Age
) : IRequest<Result<bool>>;
