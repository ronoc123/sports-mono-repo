using Application.Common.Models;
using MediatR;

namespace Application.Codes.Commands.RedeemCode;

public record RedeemCodeCommand(Guid CodeId, Guid UserId) : IRequest<Result<bool>>;
