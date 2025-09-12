using Application.Common.Models;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Codes.Commands.GenerateCode;

public record GenerateCodeCommand(OrganizationId OrganizationId) : IRequest<Result<Guid>>;
