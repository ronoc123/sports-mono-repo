using Application.Common.Models;
using MediatR;

namespace Application.Codes.Commands.GenerateCode;

public record GenerateCodeCommand(Guid OrganizationId) : IRequest<Result<Guid>>;
