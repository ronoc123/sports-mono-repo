using Application.Common.Models;
using MediatR;

namespace Application.PlayerOptions.Commands.UpdatePlayerOption;

public record UpdatePlayerOptionCommand(
    Guid PlayerOptionId,
    string Title,
    string Description
) : IRequest<Result<bool>>;
