using Domain.Cards;
using Domain.Enums;
using Domain.Leagues;
using Domain.Organizations;
using Domain.Player;
using Domain.PlayerOption;
using Domain.Product;
using Domain.ValueObjects;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace Infrastructure.Data
{
    public class SportsDbImporter
    {
        private readonly HttpClient _http;

        public SportsDbImporter(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<SportsDbTeamDto>> GetTeamsByLeague(string leagueId)
        {
            var response = await _http.GetFromJsonAsync<TeamsResponse>(
                $"search_all_teams.php?id={leagueId}");

            return response?.Teams ?? new List<SportsDbTeamDto>();
        }

        public async Task<List<SportsDbPlayerDto>> GetPlayersForTeam(string teamId)
        {
            var response = await _http.GetFromJsonAsync<PlayersResponse>(
                $"lookup_all_players.php?id={teamId}");

            return response?.Player ?? new List<SportsDbPlayerDto>();
        }
    }

    #region DTOs

    public class SportsDbTeamDto
    {
        public string idTeam { get; set; }
        public string strTeam { get; set; }
        public string strTeamShort { get; set; }
        public string intFormedYear { get; set; }
        public string strSport { get; set; }
        public string strStadium { get; set; }
        public string strLocation { get; set; }
        public string intStadiumCapacity { get; set; }
        public string strTeamBadge { get; set; }
        public string strTeamLogo { get; set; }
        public string strTeamFanart1 { get; set; }
        public string strTeamFanart2 { get; set; }
        public string strTeamFanart3 { get; set; }
        public string strWebsite { get; set; }
        public string strFacebook { get; set; }
        public string strTwitter { get; set; }
        public string strInstagram { get; set; }
        public string strColour1 { get; set; }
        public string strColour2 { get; set; }
        public string strColour3 { get; set; }
        public string strDescriptionEN { get; set; }
    }

    internal class TeamsResponse
    {
        public List<SportsDbTeamDto> Teams { get; set; }
    }

    public class SportsDbPlayerDto
    {
        public string idPlayer { get; set; }
        public string strPlayer { get; set; }
        public string strPosition { get; set; }
        public string dateBorn { get; set; }
        public string strThumb { get; set; }
        public string strDescriptionEN { get; set; }
    }

    internal class PlayersResponse
    {
        public List<SportsDbPlayerDto> Player { get; set; }
    }

    #endregion

    public static class SeedData
    {
        public static async Task SeedAsync(SportsDbAppContext context, SportsDbImporter importer)
        {

            if (await context.Organizations.AnyAsync())
                return;

            await SeedVoteBundles(context);
            var leagues = await SeedLeagues(context);
            var orgs = await SeedOrganizations(context, leagues, importer);
            await SeedRarityTierConfigs(context, leagues);
            var players = await SeedPlayers(context, orgs, importer);
            await SeedPlayerOptions(context, players);

            var nflLeague = leagues.First(l => l.Name == "NFL");
            var nbaLeague = leagues.First(l => l.Name == "NBA");
            var filePath = Path.Combine(AppContext.BaseDirectory, "Data", "player_ratings_2015_2025.csv");
            var nbaFile = Path.Combine(AppContext.BaseDirectory, "Data", "player_overalls.csv");
            await SeedCardPlayersFromCsv(context, nflLeague.Id.Value, filePath);
            await SeedCardPlayersFromCsv(context, nbaLeague.Id.Value, nbaFile);
        }

        /// <summary>
        /// Seeds 4 rarity tiers for every league. Pull weights sum to 10,000 bps (100%).
        /// Thresholds: Common 60-74 (50%), Rare 75-84 (30%), Epic 85-94 (15%), Legendary 95-99 (5%).
        /// </summary>
        private static async Task SeedRarityTierConfigs(SportsDbAppContext context, List<League> leagues)
        {
            var configs = new List<RarityTierConfig>();

            foreach (var league in leagues)
            {
                var leagueGuid = league.Id.Value;
                configs.AddRange(new[]
                {
                    RarityTierConfig.Create(leagueGuid, "Common",    60, 74, 5000),
                    RarityTierConfig.Create(leagueGuid, "Rare",      75, 84, 3000),
                    RarityTierConfig.Create(leagueGuid, "Epic",      85, 94, 1500),
                    RarityTierConfig.Create(leagueGuid, "Legendary", 95, 99,  500),
                });
            }

            context.RarityTierConfigs.AddRange(configs);
            await context.SaveChangesAsync();
        }

        private static async Task<List<League>> SeedLeagues(SportsDbAppContext context)
        {
            var leagues = new List<League>
            {
                League.Create("NFL"),
                League.Create("NBA")
            };

            context.Leagues.AddRange(leagues);
            await context.SaveChangesAsync();

            return leagues;
        }

        private static async Task<List<Organization>> SeedOrganizations(SportsDbAppContext context, List<League> leagues, SportsDbImporter importer)
        {
            var leagueApiMap = new Dictionary<string, string>
            {
                { "NFL", "4391" }, // NFL
                { "NBA", "4387" }  // NBA
            };

            var orgs = new List<Organization>();

            foreach (var league in leagues)
            {
                if (!leagueApiMap.TryGetValue(league.Name, out var apiLeagueId))
                    continue;

                var teams = await importer.GetTeamsByLeague(apiLeagueId);

                foreach (var team in teams)
                {
                    var formedYear = int.TryParse(team.intFormedYear, out var fy) ? fy : 1900;
                    var capacity = int.TryParse(team.intStadiumCapacity, out var cap) ? cap : 0;

                    var org = Organization.Create(
                        league.Id,
                        team.strTeam,
                        teamId: team.idTeam,
                        teamName: team.strTeam,
                        teamShortName: team.strTeamShort ?? team.strTeam,
                        formedYear: formedYear,
                        sport: team.strSport,
                        stadium: team.strStadium,
                        location: team.strLocation,
                        capacity: capacity,
                        badgeUrl: team.strTeamBadge,
                        logoUrl: team.strTeamLogo,
                        fanart1Url: team.strTeamFanart1,
                        fanart2Url: team.strTeamFanart2,
                        fanart3Url: team.strTeamFanart3,
                        website: team.strWebsite,
                        facebook: team.strFacebook,
                        twitter: team.strTwitter,
                        instagram: team.strInstagram,
                        color1: team.strColour1,
                        color2: team.strColour2,
                        color3: team.strColour3,
                        description: team.strDescriptionEN
                    );

                    orgs.Add(org);
                }
            }

            context.Organizations.AddRange(orgs);
            await context.SaveChangesAsync();

            return orgs;
        }

        private static async Task<List<Player>> SeedPlayers(SportsDbAppContext context, List<Organization> orgs, SportsDbImporter importer)
        {
            var players = new List<Player>();

            foreach (var org in orgs)
            {
                if (string.IsNullOrWhiteSpace(org.TeamId))
                    continue;

                var teamPlayers = await importer.GetPlayersForTeam(org.TeamId);

                foreach (var p in teamPlayers)
                {
                    if (string.IsNullOrWhiteSpace(p?.strPlayer))
                        continue;

                    var age = CalculateAge(p.dateBorn);

                    try
                    {
                        var player = Player.Create(
                            id: PlayerId.Of(Guid.NewGuid()),
                            leagueId: org.LeagueId,
                            name: p.strPlayer,
                            position: p.strPosition ?? "Unknown",
                            imageUrl: p.strThumb,
                            age: age,
                            organizationId: org.Id
                        );

                        players.Add(player);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"❌ Failed to create player {p.strPlayer} for {org.Name}: {ex.Message}");
                    }
                }
            }

            context.Players.AddRange(players);
            await context.SaveChangesAsync();

            return players;
        }

        private static int CalculateAge(string dateBorn)
        {
            if (!DateTime.TryParse(dateBorn, out var dob))
                return 0;

            var today = DateTime.UtcNow;
            var age = today.Year - dob.Year;

            if (dob.Date > today.AddYears(-age))
                age--;

            return age;
        }

        private static async Task SeedPlayerOptions(SportsDbAppContext context, List<Player> players)
        {
            var options = new List<PlayerOption>();

            foreach (var p in players)
            {
                for (int i = 0; i < 2; i++)
                {
                    var expires = DateTime.UtcNow.AddDays(30 + i * 15);

                    options.Add(
                        PlayerOption.Create(
                            title: i == 0 ? "Extend Contract" : "Trade Option",
                            description: "Automatically generated player option.",
                            playerId: p.Id,
                            organizationId: p.OrganizationId!,
                            expiresAt: expires
                        )
                    );
                }
            }

            context.PlayerOptions.AddRange(options);
            await context.SaveChangesAsync();
        }

        private static async Task SeedVoteBundles(SportsDbAppContext context)
        {
            if (await context.Products.AnyAsync(p => p.Type == ProductType.Votes))
                return;

            var bundles = new List<Product>
            {
                Product.Create("Starter Pack", "10 votes for your organization",
                    ProductType.Votes, 10, new Money(0.99m, "USD")),

                Product.Create("Fan Pack", "50 votes for your organization",
                    ProductType.Votes, 50, new Money(3.99m, "USD")),

                Product.Create("Superfan Pack", "100 votes for your organization",
                    ProductType.Votes, 100, new Money(6.99m, "USD")),

                Product.Create("Ultimate Pack", "200 votes for your organization",
                    ProductType.Votes, 200, new Money(9.99m, "USD")),
            };

            context.Products.AddRange(bundles);
            await context.SaveChangesAsync();
        }

        private static async Task SeedCardPlayersFromCsv(SportsDbAppContext context, Guid leagueId, string filePath)
        {
            if (!File.Exists(filePath))
                throw new Exception($"Card seed file not found: {filePath}");

            var lines = await File.ReadAllLinesAsync(filePath);

            if (lines.Length <= 1)
                return;

            var cards = new List<CardPlayer>();

            // Skip header
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');

                if (parts.Length < 3)
                    continue;

                var name = parts[0].Trim();
                var position = parts[1].Trim();

                if (!int.TryParse(parts[2], out var rating))
                    continue;

                var rarity = DetermineRarity(rating);

                try
                {
                    var card = CardPlayer.Create(
                        leagueId,
                        name,
                        position,
                        rating,
                        rarity
                    );

                    cards.Add(card);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to create card {name}: {ex.Message}");
                }
            }

            context.CardPlayers.AddRange(cards);
            await context.SaveChangesAsync();
        }

        private static string DetermineRarity(int rating)
        {
            if (rating >= 95) return "Legendary";
            if (rating >= 85) return "Epic";
            if (rating >= 75) return "Rare";
            return "Common";
        }
    }
}
