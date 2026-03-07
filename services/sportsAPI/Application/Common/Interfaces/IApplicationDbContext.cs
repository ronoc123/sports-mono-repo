using Domain.Cards;
using Domain.H2H;
using Domain.Leagues;
using Domain.Marketplace;
using Domain.Organizations;
using Domain.Organizations.Entities;
using Domain.Player;
using Domain.PlayerOption;
using DomainPoll = Domain.Poll;
using Domain.Trivia;
using Domain.Users;
using Domain.VoteAccount;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Organization> Organizations { get; }
    DbSet<League> Leagues { get; }
    DbSet<Player> Players { get; }
    DbSet<PlayerOption> PlayerOptions { get; }
    DbSet<Theme> Themes { get; }
    DbSet<VoteAccount> VoteAccounts { get; }

    // Fan Economy — Cards
    DbSet<RarityTierConfig> RarityTierConfigs { get; }
    DbSet<PackConfig> PackConfigs { get; }
    DbSet<CardPlayer> CardPlayers { get; }
    DbSet<CardOwner> CardOwners { get; }
    DbSet<CardPack> CardPacks { get; }
    DbSet<UserCard> UserCards { get; }

    // Fan Economy — Marketplace
    DbSet<AuctionListing> AuctionListings { get; }
    DbSet<Bid> Bids { get; }
    DbSet<PointsEscrow> PointsEscrows { get; }

    // Fan Economy — H2H
    DbSet<H2HMatch> H2HMatches { get; }
    DbSet<H2HSquadCard> H2HSquadCards { get; }

    // Dashboard Engagement — Trivia
    DbSet<TriviaSeries> TriviaSeries { get; }
    DbSet<TriviaAnswer> TriviaAnswers { get; }

    // Dashboard Engagement — Polls
    DbSet<DomainPoll.Poll> Polls { get; }
    DbSet<DomainPoll.PollVote> PollVotes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
