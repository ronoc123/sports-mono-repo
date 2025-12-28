using Contracts.Contracts;
using MediatR;

namespace Application.Votes.Commands.EmailReward
{
    public sealed record EmailRewardCommand(Guid UserId, int Amount, Guid OrganizationId) : IRequest<ServiceResponse<string>>;
}
