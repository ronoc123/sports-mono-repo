using Application.Dto.VoteAccount;
using Application.Votes.Commands.EmailReward;
using Application.Votes.Commands.RedeemReward;
using Application.Votes.Queries.GetVoteAccount;
using Contracts.Contracts;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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
        // GET api/<VoteAccount>/5
        [HttpGet("get-vote-account/{userId}")]
        public async Task<ServiceResponse<List<VoteAccountDto>>> GetAll([FromQuery] Guid userId)
        {
            return null;
        }

        [HttpGet("get-vote-account/{userId}/organization/{organizationId}")]
        public async Task<ServiceResponse<VoteAccountDto>> Get(Guid userId, Guid organizationId)
        {
            var query = new GetVoteAccountQuery(UserId.Of(userId), OrganizationId.Of(organizationId));

            var result = await _mediator.Send(query);
            return result;
        }

        // POST api/<VoteAccount>
        [HttpPost("redeem-vote/{userId}/reward/{promoCode}")]
        public async Task<ServiceResponse<VoteAccountDto>> Redeem(Guid userId, string promoCode)
        {
            var query = new RedeemRewardCommand(UserId.Of(userId), promoCode);

            var result = await _mediator.Send(query);
            return result;
        }

        // POST api/<VoteAccount>
        [HttpPost("reward-for-user")]
        public async Task<ServiceResponse<string>> Send([FromBody] EmailRewardRedemptionCommand command)
        {
            var result = await _mediator.Send(command);
            return result;
        }

    }
}
