using Application.Dto.VoteAccount;
using Application.Votes.Commands.EmailReward;
using Application.Votes.Commands.RedeemReward;
using Application.Votes.Queries.GetVoteAccount;
using Contracts.Contracts;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoteAccount : ControllerBase
    {
        private readonly IMediator _mediator;
        public VoteAccount(IMediator meduator)
        {
            _mediator = meduator;
        }

        [HttpGet("get-vote-account/{userId}/league/{leagueId}")]
        public async Task<ServiceResponse<VoteAccountDto>> Get(Guid userId, Guid leagueId)
        {
            var query = new GetVoteAccountQuery(UserId.Of(userId), LeagueId.Of(leagueId));

            var result = await _mediator.Send(query);
            return result;
        }

        [HttpPost("redeem-vote/{userId}/reward/{promoCode}")]
        public async Task<ServiceResponse<VoteAccountDto>> Redeem(Guid userId, string promoCode, [FromQuery] Guid leagueId)
        {
            var command = new RedeemRewardCommand(UserId.Of(userId), promoCode, LeagueId.Of(leagueId));

            var result = await _mediator.Send(command);
            return result;
        }

        [HttpPost("reward-for-user")]
        public async Task<ServiceResponse<string>> Send([FromBody] EmailRewardRedemptionCommand command)
        {
            var result = await _mediator.Send(command);
            return result;
        }
    }
}
