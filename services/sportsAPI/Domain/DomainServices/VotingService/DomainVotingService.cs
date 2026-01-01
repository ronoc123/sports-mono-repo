using Domain.DomainServices.VotingService.VotingService;


namespace Domain.DomainServices.Voting
{
    public class DomainVotingService : IDomainVotingService
    {
        public void Vote(VoteAccount.VoteAccount account, PlayerOption.PlayerOption option, UserId userId, long amount)
        {
            // 1. Authorize spend
            var token = account.AuthorizeSpend(option.Id, amount, Guid.NewGuid().ToString());

            // 2. Apply the vote
            option.CastVote(userId, token);

            // 3. Debit the account
            account.ApplySpend(token);
        }
    }

}
