using Application.Cards.Services;
using Application.Dto.Cards;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.Cards;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using Domain.VoteAccount;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cards.Commands.PurchasePack;

public sealed class PurchasePackCommandHandler
    : IRequestHandler<PurchasePackCommand, ServiceResponse<PackPurchaseResultDto>>
{
    /// <summary>Default points cost used when no PackConfig row exists for the org.</summary>
    public const long DefaultPackCost = 500;

    private readonly IRepository _repo;
    private readonly RarityEngine _rarityEngine;

    public PurchasePackCommandHandler(IRepository repo, RarityEngine rarityEngine)
    {
        _repo = repo;
        _rarityEngine = rarityEngine;
    }

    public async Task<ServiceResponse<PackPurchaseResultDto>> Handle(
        PurchasePackCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load rarity tiers for the org
        var rarityConfigs = await _repo.Query<RarityTierConfig>()
            .Where(r => r.OrgId == request.OrgId)
            .ToListAsync(cancellationToken);

        if (rarityConfigs.Count == 0)
            throw new DomainException("No rarity tiers configured for this organisation.");

        // 2. Load all CardPlayers in this league, grouped by rarity tier
        var cardPlayers = await _repo.Query<CardPlayer>()
            .Where(c => c.LeagueId == request.LeagueId)
            .ToListAsync(cancellationToken);

        if (cardPlayers.Count == 0)
            throw new DomainException("No card players exist in this league's catalog.");

        var cardsByTier = cardPlayers
            .GroupBy(c => c.RarityTier)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<CardPlayer>)g.ToList());

        var buckets = rarityConfigs
            .Select(cfg => new RarityEngine.Bucket(
                cfg,
                cardsByTier.TryGetValue(cfg.RarityName, out var cards) ? cards : []))
            .Where(b => b.Cards.Count > 0 || b.Config.PullWeightBps > 0)
            .ToList();

        if (buckets.All(b => b.Cards.Count == 0))
            throw new DomainException("No eligible cards found for any rarity tier.");

        // 3. Resolve pack cost from org config (fall back to default if not yet configured)
        var packConfig = await _repo.Query<PackConfig>()
            .FirstOrDefaultAsync(p => p.OrgId == request.OrgId, cancellationToken);
        var packCost = packConfig?.PackPointCost ?? DefaultPackCost;

        // 4. Load VoteAccount and validate balance (throws DomainException → 422 if insufficient)
        var account = await _repo.GetByIdAsync<VoteAccount>(
            cancellationToken,
            OrganizationId.Of(request.OrgId),
            UserId.Of(request.UserId));

        if (account is null)
            throw new DomainException("Vote account not found. Please ensure your account is set up.");

        // 5. Execute weighted pull (before deducting — pure computation, no side effects yet)
        var (seed, pullResults) = _rarityEngine.Pull(buckets);

        if (pullResults.Count == 0)
            throw new DomainException("Pull engine produced no results.");

        // 5. Create CardPack aggregate
        var pack = CardPack.Create(
            request.UserId,
            request.OrgId,
            request.LeagueId,
            packCost,
            seed);

        // 6. Deduct points atomically — throws DomainException with code INSUFFICIENT_BALANCE if low
        account.DeductForPackPurchase(packCost, pack.Id.ToString());

        // 7. Create UserCard records (pull log + ownership)
        var userCards = pullResults
            .Select(r => UserCard.Create(
                pack.Id,
                request.UserId,
                r.Card.Id,
                request.OrgId,
                request.LeagueId,
                r.RarityTier,
                seed))
            .ToList();

        // 8. Persist — all in one SaveChanges (atomic)
        await _repo.AddAsync(pack, cancellationToken);
        await _repo.AddRangeAsync(userCards, cancellationToken);
        _repo.Update(account);
        await _repo.SaveChangesAsync(cancellationToken);

        // 9. Build response (rarity weights deliberately excluded)
        var cardDtos = userCards.Zip(pullResults, (uc, pr) => new UserCardDto
        {
            Id = uc.Id,
            CardPackId = uc.CardPackId,
            CardPlayerId = uc.CardPlayerId,
            LeagueId = uc.LeagueId,
            Name = pr.Card.Name,
            Position = pr.Card.Position,
            OverallRating = pr.Card.OverallRating,
            RarityTier = uc.RarityTier,
            PulledAt = uc.CreatedAt,
        }).ToList();

        var result = new PackPurchaseResultDto
        {
            PackId = pack.Id,
            PointsSpent = packCost,
            RemainingBalance = account.Balance,
            Cards = cardDtos,
        };

        return ServiceResponse.Ok(result, "Pack purchased successfully. 5 cards added to your collection.");
    }
}
