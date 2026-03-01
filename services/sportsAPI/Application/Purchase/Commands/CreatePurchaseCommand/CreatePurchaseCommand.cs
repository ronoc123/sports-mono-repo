using Contracts.Contracts;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Purchase.Commands.CreatePurchaseCommand
{
    public record CreatePurchaseCommand(
        UserId UserId,
        OrganizationId OrgId,
        Guid ProductId) : IRequest<ServiceResponse<CreatePurchaseResponse>>;
}
