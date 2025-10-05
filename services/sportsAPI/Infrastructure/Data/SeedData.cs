using Microsoft.EntityFrameworkCore;
using Domain.Organizations;
using Domain.Organizations.Entities;
using Domain.Users;
using Domain.User.Entities;
using Domain.Leagues;
using Domain.SharedKernal;
using Domain.ValueObjects.ConcreteTypes;
using Domain.ValueObjects;

namespace Infrastructure.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(SportsDbAppContext context)
        {
            try
            {
                // Check if data already exists
                if (await context.Organizations.AnyAsync())
                {
                    Console.WriteLine("🔄 Database already seeded, skipping...");
                    return; // Database has been seeded
                }

                Console.WriteLine("🌱 Starting database seeding...");

                // Seed Leagues first
                await SeedLeagues(context);
                Console.WriteLine("✅ Leagues seeded");

                // Seed simplified NFL Organizations (without complex value objects)
                await SeedSimpleNFLOrganizations(context);
                Console.WriteLine("✅ NFL Organizations seeded");

                Console.WriteLine("🎉 Basic seed data created successfully!");
                Console.WriteLine("📊 Seeded: 1 League (NFL), 1 Organization (Kansas City Chiefs)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Seeding failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                // Don't throw - let the app continue running
            }
        }

        private static async Task SeedLeagues(SportsDbAppContext context)
        {
            var leagues = new List<League>
            {
                League.Create(
                    LeagueId.Of(Guid.Parse("11111111-1111-1111-1111-111111111111")),
                    "NFL"
                ),
                League.Create(
                    LeagueId.Of(Guid.Parse("22222222-2222-2222-2222-222222222222")),
                    "NBA"
                ),
                League.Create(
                    LeagueId.Of(Guid.Parse("33333333-3333-3333-3333-333333333333")),
                    "MLB"
                )
            };

            context.Leagues.AddRange(leagues);
            await context.SaveChangesAsync();
        }

        private static async Task SeedSimpleNFLOrganizations(SportsDbAppContext context)
        {
            var nflLeagueId = LeagueId.Of(Guid.Parse("11111111-1111-1111-1111-111111111111"));

            Console.WriteLine("🏈 Creating Kansas City Chiefs...");

            // Start with just one team to test
            var chiefs = CreateSimpleNFLTeam(
                nflLeagueId,
                "080C64B5-45F7-7888-8397-770799010899",
                "Kansas City Chiefs",
                "KC",
                "Chiefs"
            );

            context.Organizations.Add(chiefs);
            await context.SaveChangesAsync();

            Console.WriteLine("✅ Kansas City Chiefs created successfully!");
        }

        private static Organization CreateSimpleNFLTeam(LeagueId leagueId, string id, string name, string abbreviation, string nickname)
        {
            return Organization.Create(
                OrganizationId.Of(Guid.Parse(id)),
                leagueId,
                name,
                abbreviation,
                nickname,
                abbreviation,
                1960, // Default founding year
                "Football",
                new Venue("Stadium", "City, State", 70000), // Default venue
                new MediaAssets("", "", "", "", ""), // Empty media assets
                new SocialLinks("", "", "", ""), // Empty social links
                new TeamColors("#000000", "#FFFFFF", "#CCCCCC"), // Default colors
                $"The {name} are a professional American football team."
            );
        }

        private static async Task SeedNFLOrganizations(SportsDbAppContext context)
        {
            var nflLeagueId = LeagueId.Of(Guid.Parse("11111111-1111-1111-1111-111111111111"));

            var nflTeams = new List<Organization>();

            // AFC East
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "3B60378E-782A-45D5-B6C4-AA7466F8D5FD", "Buffalo Bills", "BUF", "Bills", 1960, "Highmark Stadium", "Orchard Park, NY", 71608, "#00338D", "#C60C30", "#FFFFFF", "https://buffalobills.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "4C70489F-893B-56E6-C7D5-BB8577F9E6FE", "Miami Dolphins", "MIA", "Dolphins", 1966, "Hard Rock Stadium", "Miami Gardens, FL", 65326, "#008E97", "#FC4C02", "#FFFFFF", "https://miamidolphins.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "5D81590A-9A4C-67F7-D8E6-CC9688F0F7FF", "New England Patriots", "NE", "Patriots", 1960, "Gillette Stadium", "Foxborough, MA", 65878, "#002244", "#C60C30", "#B0B7BC", "https://patriots.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "6E926A1B-AB5D-7818-E9F7-DD0799010808", "New York Jets", "NYJ", "Jets", 1960, "MetLife Stadium", "East Rutherford, NJ", 82500, "#125740", "#FFFFFF", "#000000", "https://newyorkjets.com"));

            // AFC North
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "7FA37B2C-BC6E-8919-FA08-EE1800020909", "Baltimore Ravens", "BAL", "Ravens", 1996, "M&T Bank Stadium", "Baltimore, MD", 71008, "#241773", "#000000", "#9E7C0C", "https://baltimoreravens.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "80B48C3D-CD7F-9010-0B19-FF2911030011", "Cincinnati Bengals", "CIN", "Bengals", 1968, "Paycor Stadium", "Cincinnati, OH", 65515, "#FB4F14", "#000000", "#FFFFFF", "https://bengals.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "91C59D4E-DE80-0111-1C20-003022040122", "Cleveland Browns", "CLE", "Browns", 1946, "Cleveland Browns Stadium", "Cleveland, OH", 67431, "#311D00", "#FF3C00", "#FFFFFF", "https://clevelandbrowns.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "A2D60E5F-EF91-1222-2D31-114133050233", "Pittsburgh Steelers", "PIT", "Steelers", 1933, "Heinz Field", "Pittsburgh, PA", 68400, "#FFB612", "#101820", "#C60C30", "https://steelers.com"));

            // AFC South
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "B3E71F60-F0A2-2333-3E42-225244060344", "Houston Texans", "HOU", "Texans", 2002, "NRG Stadium", "Houston, TX", 72220, "#03202F", "#A71930", "#FFFFFF", "https://houstontexans.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "C4F82071-01B3-3444-4F53-336355070455", "Indianapolis Colts", "IND", "Colts", 1953, "Lucas Oil Stadium", "Indianapolis, IN", 67000, "#002C5F", "#A2AAAD", "#FFFFFF", "https://colts.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "D5093182-12C4-4555-5064-447466080566", "Jacksonville Jaguars", "JAX", "Jaguars", 1995, "TIAA Bank Field", "Jacksonville, FL", 67431, "#101820", "#D7A22A", "#9F792C", "https://jaguars.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "E60A4293-23D5-5666-6175-558577090677", "Tennessee Titans", "TEN", "Titans", 1960, "Nissan Stadium", "Nashville, TN", 69143, "#0C2340", "#4B92DB", "#C8102E", "https://titansonline.com"));

            // AFC West
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "F70B53A4-34E6-6777-7286-669688000788", "Denver Broncos", "DEN", "Broncos", 1960, "Empower Field at Mile High", "Denver, CO", 76125, "#FB4F14", "#002244", "#FFFFFF", "https://denverbroncos.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "080C64B5-45F7-7888-8397-770799010899", "Kansas City Chiefs", "KC", "Chiefs", 1960, "Arrowhead Stadium", "Kansas City, MO", 76416, "#E31837", "#FFB81C", "#FFFFFF", "https://chiefs.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "190D75C6-5608-8999-9408-881800020900", "Las Vegas Raiders", "LV", "Raiders", 1960, "Allegiant Stadium", "Las Vegas, NV", 65000, "#000000", "#A5ACAF", "#FFFFFF", "https://raiders.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "2A0E86D7-6719-9000-A519-992911030011", "Los Angeles Chargers", "LAC", "Chargers", 1960, "SoFi Stadium", "Los Angeles, CA", 70240, "#0080C6", "#FFC20E", "#FFFFFF", "https://chargers.com"));

            // NFC East
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "3B0F97E8-782A-0111-B620-AA3022040122", "Dallas Cowboys", "DAL", "Cowboys", 1960, "AT&T Stadium", "Arlington, TX", 80000, "#003594", "#041E42", "#869397", "https://dallascowboys.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "4C1008F9-893B-1222-C731-BB4133050233", "New York Giants", "NYG", "Giants", 1925, "MetLife Stadium", "East Rutherford, NJ", 82500, "#0B2265", "#A71930", "#A5ACAF", "https://giants.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "5D211900-9A4C-2333-D842-CC5244060344", "Philadelphia Eagles", "PHI", "Eagles", 1933, "Lincoln Financial Field", "Philadelphia, PA", 69596, "#004C54", "#A5ACAF", "#ACC0C6", "https://eagles.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "6E322A00-AB5D-3444-E953-DD6355070455", "Washington Commanders", "WAS", "Commanders", 1932, "FedExField", "Landover, MD", 82000, "#5A1414", "#FFB612", "#FFFFFF", "https://commanders.com"));

            // NFC North
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "7F433B00-BC6E-4555-FA64-EE7466080566", "Chicago Bears", "CHI", "Bears", 1920, "Soldier Field", "Chicago, IL", 61500, "#0B162A", "#C83803", "#FFFFFF", "https://chicagobears.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "80544C00-CD7F-5666-0B75-FF8577090677", "Detroit Lions", "DET", "Lions", 1930, "Ford Field", "Detroit, MI", 65000, "#0076B6", "#B0B7BC", "#000000", "https://detroitlions.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "91655D00-DE80-6777-1C86-008688000788", "Green Bay Packers", "GB", "Packers", 1919, "Lambeau Field", "Green Bay, WI", 81441, "#203731", "#FFB612", "#FFFFFF", "https://packers.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "A2766E00-EF91-7888-2D97-119799010899", "Minnesota Vikings", "MIN", "Vikings", 1961, "U.S. Bank Stadium", "Minneapolis, MN", 66860, "#4F2683", "#FFC62F", "#FFFFFF", "https://vikings.com"));

            // NFC South
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "B3877F00-F0A2-8999-3EA8-220800020900", "Atlanta Falcons", "ATL", "Falcons", 1966, "Mercedes-Benz Stadium", "Atlanta, GA", 71000, "#A71930", "#000000", "#A5ACAF", "https://atlantafalcons.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "C4988000-01B3-9000-4FB9-331911030011", "Carolina Panthers", "CAR", "Panthers", 1995, "Bank of America Stadium", "Charlotte, NC", 75523, "#0085CA", "#101820", "#BFC0BF", "https://panthers.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "D5A99000-12C4-0111-50CA-442022040122", "New Orleans Saints", "NO", "Saints", 1967, "Caesars Superdome", "New Orleans, LA", 73208, "#D3BC8D", "#101820", "#FFFFFF", "https://neworleanssaints.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "E6BAA000-23D5-1222-61DB-553133050233", "Tampa Bay Buccaneers", "TB", "Buccaneers", 1976, "Raymond James Stadium", "Tampa, FL", 65890, "#D50A0A", "#FF7900", "#0A0A08", "https://buccaneers.com"));

            // NFC West
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "F7CBB000-34E6-2333-72EC-664244060344", "Arizona Cardinals", "ARI", "Cardinals", 1898, "State Farm Stadium", "Glendale, AZ", 63400, "#97233F", "#000000", "#FFB612", "https://azcardinals.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "08DCC000-45F7-3444-83FD-775355070455", "Los Angeles Rams", "LAR", "Rams", 1936, "SoFi Stadium", "Los Angeles, CA", 70240, "#003594", "#FFA300", "#FF8200", "https://therams.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "19EDD000-5608-4555-940E-886466080566", "San Francisco 49ers", "SF", "49ers", 1946, "Levi's Stadium", "Santa Clara, CA", 68500, "#AA0000", "#B3995D", "#FFFFFF", "https://49ers.com"));
            nflTeams.Add(CreateNFLTeam(nflLeagueId, "2AFEE000-6719-5666-A51F-997577090677", "Seattle Seahawks", "SEA", "Seahawks", 1976, "Lumen Field", "Seattle, WA", 69000, "#002244", "#69BE28", "#A5ACAF", "https://seahawks.com"));

            context.Organizations.AddRange(nflTeams);
            await context.SaveChangesAsync();
        }

        private static Organization CreateNFLTeam(LeagueId leagueId, string id, string name, string abbreviation, string nickname, int founded, string stadium, string location, int capacity, string primaryColor, string secondaryColor, string tertiaryColor, string website)
        {
            return Organization.Create(
                OrganizationId.Of(Guid.Parse(id)),
                leagueId,
                name,
                abbreviation,
                nickname,
                abbreviation,
                founded,
                "Football",
                new Venue(stadium, location, capacity),
                new MediaAssets(
                    $"https://example.com/{abbreviation.ToLower()}-logo.png",
                    $"https://example.com/{abbreviation.ToLower()}-banner.jpg",
                    $"https://example.com/{abbreviation.ToLower()}-fanart1.jpg",
                    $"https://example.com/{abbreviation.ToLower()}-fanart2.jpg",
                    $"https://example.com/{abbreviation.ToLower()}-fanart3.jpg"
                ),
                new SocialLinks(
                    $"https://twitter.com/{abbreviation.ToLower()}",
                    $"https://facebook.com/{abbreviation.ToLower()}",
                    $"https://instagram.com/{abbreviation.ToLower()}",
                    website
                ),
                new TeamColors(primaryColor, secondaryColor, tertiaryColor),
                $"The {name} are a professional American football team."
            );
        }


    }
}
