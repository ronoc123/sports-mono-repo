using Contracts.Contracts;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Purchase.Commands.CreatePurchaseCommand
{

    public record CreatePurchaseCommand(UserId UserId, OrganizationId OrgId, string ItemType, int Quantity) : IRequest<ServiceResponse<Guid>>;
}
