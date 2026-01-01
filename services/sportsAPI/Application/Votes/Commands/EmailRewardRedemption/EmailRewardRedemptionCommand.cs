using Contracts.Contracts;
using MediatR;

namespace Application.Votes.Commands.EmailRewardRedemption
{
    public sealed record EmailRewardRedemptionCommand(Guid UserId, int Amount, Guid OrganizationId) : IRequest<ServiceResponse<string>>;
}
