using Application.Common.Models;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Codes.Commands.RedeemCode;

public record RedeemCodeCommand(CodeId CodeId, UserId UserId) : IRequest<Result<bool>>;
