using Application.H2H.Commands.ResolveMatch;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.Cards;
using Domain.H2H;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using Domain.VoteAccount;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.H2H.Commands.CreateMatch;

public sealed class CreateMatchCommandHandler
    : IRequestHandler<CreateMatchCommand, ServiceResponse<CreateMatchResult>>
{
    private readonly IRepository _repo;
    private readonly ISender _sender;

    public CreateMatchCommandHandler(IRepository repo, ISender sender)
    {
        _repo = repo;
        _sender = sender;
    }

    public async Task<ServiceResponse<CreateMatchResult>> Handle(
        CreateMatchCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate squad size
        if (request.UserCardIds.Count < 1 || request.UserCardIds.Count > 5)
            throw new DomainException("Squad must contain between 1 and 5 cards.");

        // 2. Load and validate UserCards (ownership + not listed)
        var userCards = await _repo.Query<UserCard>(asNoTracking: true)
            .Include(uc => uc.CardPlayer)
            .Where(uc => request.UserCardIds.Contains(uc.Id))
            .ToListAsync(cancellationToken);

        if (userCards.Count != request.UserCardIds.Count)
            throw new DomainException("One or more cards could not be found.");

        foreach (var uc in userCards)
        {
            if (uc.UserId != request.FanUserId)
                throw new DomainException("One or more cards do not belong to you.");
            if (uc.IsListed)
                throw new DomainException("One or more cards are currently listed on the marketplace.");
        }

        // 3. Determine league from squad cards
        var leagueId = userCards.First().LeagueId;

        // 4. Calculate fan team overall (average of squad ratings)
        var fanTeamOverall = (int)Math.Round(
            userCards.Average(uc => uc.CardPlayer!.OverallRating));

        // 5. Load fan VoteAccount and validate balance
        var fanAccount = await _repo.GetByIdAsync<VoteAccount>(
            cancellationToken,
            OrganizationId.Of(request.OrgId),
            UserId.Of(request.FanUserId))
            ?? throw new DomainException("Vote account not found.");

        if (fanAccount.Balance < request.WagerAmount)
            throw new DomainException("Insufficient balance.");

        // 6. Create match and squad cards
        var match = H2HMatch.Create(
            request.FanUserId,
            request.OrgId,
            leagueId,
            request.WagerAmount,
            fanTeamOverall);

        var squadCards = request.UserCardIds
            .Select((userCardId, index) => H2HSquadCard.Create(match.Id, userCardId, index))
            .ToList();

        // 7. Deduct wager from fan's account
        fanAccount.DeductForH2HWager(request.WagerAmount, match.Id.ToString());

        // 8. Persist — single SaveChanges = implicit transaction
        await _repo.AddAsync(match, cancellationToken);
        await _repo.AddRangeAsync(squadCards, cancellationToken);
        _repo.Update(fanAccount);
        await _repo.SaveChangesAsync(cancellationToken);

        // 9. Resolve match synchronously (bot assembly + outcome determination + balance update)
        await _sender.Send(new ResolveMatchCommand(match.Id), cancellationToken);

        // 10. Reload completed match to return outcome
        var completed = await _repo.Query<H2HMatch>(asNoTracking: true)
            .FirstOrDefaultAsync(m => m.Id == match.Id, cancellationToken)
            ?? throw new DomainException("Match not found after resolution.");

        var updatedAccount = await _repo.GetByIdAsync<VoteAccount>(
            cancellationToken,
            OrganizationId.Of(request.OrgId),
            UserId.Of(request.FanUserId));

        return ServiceResponse.Ok(new CreateMatchResult(
            completed.Id,
            completed.Outcome,
            completed.FanTeamOverall,
            completed.BotTeamOverall,
            completed.BotSquadSnapshot,
            updatedAccount?.Balance ?? 0,
            completed.WagerAmount
        ), "Match resolved.");
    }
}
