using Domain.Leagues;
using Domain.Organizations;
using Domain.Organizations.Entities;
using Domain.Player;
using Domain.PlayerOption;
using Domain.Users;
using Domain.ValueObjects;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(SportsDbAppContext context)
        {
          if (await context.Organizations.AnyAsync())
            return;

          var leagues = await SeedLeagues(context);
          var orgs = await SeedOrganizations(context, leagues);
          var players = await SeedPlayers(context, orgs);
          await SeedPlayerOptions(context, players, Guid.Parse("eaaa2b40-e8d2-4c2c-ad37-0d6d305b7715"));
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

        private static async Task<List<Organization>> SeedOrganizations(SportsDbAppContext context, List<League> leagues)
        {
          var nfl = leagues.First(l => l.Name == "NFL").Id;
          var nba = leagues.First(l => l.Name == "NBA").Id;

          var orgs = new List<Organization>();

          foreach (var t in SeedTeams.NFLTeams)
          {
            orgs.Add(
                Organization.Create(
                    nfl,
                    t.Name,
                    teamId: t.Name[..3].ToUpper(),      // “ARI”, “ATL”, etc.
                    teamName: t.Name,
                    teamShortName: t.Name.Split(' ').Last(),
                    formedYear: 1900,
                    sport: "Football",
                    stadium: t.Stadium,
                    location: t.City,
                    capacity: t.Capacity,
                    badgeUrl: t.logo,
                    logoUrl: t.logo,
                    fanart1Url: "",
                    fanart2Url: "",
                    fanart3Url: "",
                    website: "",
                    facebook: "",
                    twitter: "",
                    instagram: "",
                    color1: t.Color1,
                    color2: t.Color2,
                    color3: t.Color3,
                    description: $"{t.Name} team in the NFL."
                ));
          }

          // TODO extend model to support multiple sports :)

          //foreach (var t in SeedTeams.NBATeams)
          //{
          //  orgs.Add(
          //      Organization.Create(
          //          nba,
          //          t.Name,
          //          teamId: t.Name[..3].ToUpper(),
          //          teamName: t.Name,
          //          teamShortName: t.Name.Split(' ').Last(),
          //          formedYear: 1950,
          //          sport: "Basketball",
          //          stadium: t.Stadium,
          //          location: t.City,
          //          capacity: t.Capacity,
          //          badgeUrl: "",
          //          logoUrl: "",
          //          fanart1Url: "",
          //          fanart2Url: "",
          //          fanart3Url: "",
          //          website: "",
          //          facebook: "",
          //          twitter: "",
          //          instagram: "",
          //          color1: t.Color1,
          //          color2: t.Color2,
          //          color3: t.Color3,
          //          description: $"{t.Name} team in the NBA."
          //      ));
          //}

          context.Organizations.AddRange(orgs);
          await context.SaveChangesAsync();

          return orgs;
        }

        private static async Task<List<Player>> SeedPlayers(SportsDbAppContext context, List<Organization> orgs)
        {
          var random = new Random();
          var positions = new[] {"QB", "RB", "WR", "TE", "LB", "CB", "DL" };

          var players = new List<Player>();

          foreach (var org in orgs)
          {
            for (int i = 0; i < 10; i++)
            {
              var playerId = PlayerId.Of(Guid.NewGuid());

              players.Add(
                  Player.Create(
                      id: playerId,
                      leagueId: org.LeagueId,
                      name: $"Player {org.Name} {i + 1}",
                      position: positions[random.Next(positions.Length)],
                      imageUrl: "https://picsum.photos/200",
                      age: random.Next(18, 38),
                      organizationId: org.Id
                  )
              );
            }
          }

          context.Players.AddRange(players);
          await context.SaveChangesAsync();

          return players;
        }


        private static async Task SeedPlayerOptions(SportsDbAppContext context, List<Player> players, Guid userId)
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

  }

  public static class SeedTeams
    {
      public static readonly (string Name, string City, string Stadium, int Capacity, string Color1, string Color2, string Color3, string logo)[] NFLTeams =
      {
        ("Arizona Cardinals", "Glendale, Arizona", "State Farm Stadium", 63400, "#97233F", "#FFB612", "#000000", "arizona-cardinals.png"),
        ("Atlanta Falcons", "Atlanta, Georgia", "Mercedes-Benz Stadium", 71000, "#A71930", "#000000", "#A5ACAF", "atlanta-falcons.png"),
        ("Baltimore Ravens", "Baltimore, Maryland", "M&T Bank Stadium", 71000, "#241773", "#000000", "#9E7C0C", "default-player.jpg"),
        ("Buffalo Bills", "Orchard Park, New York", "Highmark Stadium", 71608, "#00338D", "#C60C30", "#FFFFFF", "buffalo-bills.png"),
        ("Carolina Panthers", "Charlotte, North Carolina", "Bank of America Stadium", 75419, "#0085CA", "#101820", "#BFC0BF", "carolina-panthers.png"),
        ("Chicago Bears", "Chicago, Illinois", "Soldier Field", 61500, "#0B162A", "#C83803", "#FFFFFF", "chicago-bears.png"),
        ("Cincinnati Bengals", "Cincinnati, Ohio", "Paycor Stadium", 65515, "#FB4F14", "#000000", "#FFFFFF", "default-player.jpg"),
        ("Cleveland Browns", "Cleveland, Ohio", "Cleveland Browns Stadium", 67431, "#311D00", "#FF3C00", "#FFFFFF", "default-player.jpg"),
        ("Dallas Cowboys", "Arlington, Texas", "AT&T Stadium", 80000, "#003594", "#041E42", "#869397", "dallas-cowboys.png"),
        ("Denver Broncos", "Denver, Colorado", "Empower Field at Mile High", 76125, "#002244", "#FB4F14", "#FFFFFF", "default-player.jpg"),
        ("Detroit Lions", "Detroit, Michigan", "Ford Field", 65000, "#0076B6", "#B0B7BC", "#000000", "detroit-lions.png"),
        ("Green Bay Packers", "Green Bay, Wisconsin", "Lambeau Field", 81441, "#203731", "#FFB612", "#FFFFFF", "green-bay-packers.png"),
        ("Houston Texans", "Houston, Texas", "NRG Stadium", 72220, "#03202F", "#A71930", "#FFFFFF", "default-player.jpg"),
        ("Indianapolis Colts", "Indianapolis, Indiana", "Lucas Oil Stadium", 67000, "#002C5F", "#A2AAAD", "#FFFFFF", "default-player.jpg"),
        ("Jacksonville Jaguars", "Jacksonville, Florida", "EverBank Stadium", 67838, "#006778", "#000000", "#9F792C", "default-player.jpg"),
        ("Kansas City Chiefs", "Kansas City, Missouri", "Arrowhead Stadium", 76416, "#E31837", "#FFB81C", "#FFFFFF", "default-player.jpg"),
        ("Las Vegas Raiders", "Las Vegas, Nevada", "Allegiant Stadium", 65000, "#000000", "#A5ACAF", "#FFFFFF", "default-player.jpg"),
        ("Los Angeles Chargers", "Inglewood, California", "SoFi Stadium", 70240, "#0080C6", "#FFC20E", "#FFFFFF", "default-player.jpg"),
        ("Los Angeles Rams", "Inglewood, California", "SoFi Stadium", 70240, "#003594", "#FFD100", "#FFFFFF", "rams-logo.png"),
        ("Miami Dolphins", "Miami Gardens, Florida", "Hard Rock Stadium", 65326, "#008E97", "#FC4C02", "#005778", "miami-dolphins.png"),
        ("Minnesota Vikings", "Minneapolis, Minnesota", "U.S. Bank Stadium", 66655, "#4F2683", "#FFC62F", "#000000", "minnesota-vikings.png"),
        ("New England Patriots", "Foxborough, Massachusetts", "Gillette Stadium", 65878, "#002244", "#C60C30", "#B0B7BC", "new-england-patriots.png"),
        ("New Orleans Saints", "New Orleans, Louisiana", "Caesars Superdome", 73208, "#D3BC8D", "#101820", "#FFFFFF", "new-orleans-saints.png"),
        ("New York Giants", "East Rutherford, New Jersey", "MetLife Stadium", 82500, "#0B2265", "#A71930", "#FFFFFF", "default-player.jpg"),
        ("New York Jets", "East Rutherford, New Jersey", "MetLife Stadium", 82500, "#125740", "#000000", "#FFFFFF", "default-player.jpg"),
        ("Philadelphia Eagles", "Philadelphia, Pennsylvania", "Lincoln Financial Field", 69596, "#004C54", "#A5ACAF", "#000000", "default-player.jpg"),
        ("Pittsburgh Steelers", "Pittsburgh, Pennsylvania", "Acrisure Stadium", 68400, "#FFB612", "#101820", "#FFFFFF", "pittsburgh-steelers.png"),
        ("San Francisco 49ers", "Santa Clara, California", "Levi's Stadium", 68500, "#AA0000", "#B3995D", "#FFFFFF", "default-player.jpg"),
        ("Seattle Seahawks", "Seattle, Washington", "Lumen Field", 68000, "#002244", "#69BE28", "#A5ACAF", "seattle-seahawks.png"),
        ("Tampa Bay Buccaneers", "Tampa, Florida", "Raymond James Stadium", 65890, "#D50A0A", "#34302B", "#000000", "default-player.jpg"),
        ("Tennessee Titans", "Nashville, Tennessee", "Nissan Stadium", 69143, "#4B92DB", "#0C2340", "#A2AAAD", "default-player.jpg"),
        ("Washington Commanders", "Landover, Maryland", "FedExField", 82000, "#5A1414", "#FFB612", "#000000", "washington-commanders.png"),
      };

      public static readonly (string Name, string City, string Stadium, int Capacity, string Color1, string Color2, string Color3, string logo)[] NBATeams =
      {
          ("Atlanta Hawks", "Atlanta, Georgia", "State Farm Arena", 18118, "#E03A3E", "#C1D32F", "#26282A", ""),
          ("Boston Celtics", "Boston, Massachusetts", "TD Garden", 19780, "#007A33", "#BA9653", "#963821", ""),
          ("Brooklyn Nets", "Brooklyn, New York", "Barclays Center", 17732, "#000000", "#FFFFFF", "#9D9D9D", ""),
          ("Charlotte Hornets", "Charlotte, North Carolina", "Spectrum Center", 19077, "#1D1160", "#00788C", "#A1A1A4", ""),
          ("Chicago Bulls", "Chicago, Illinois", "United Center", 20917, "#CE1141", "#000000", "#FFFFFF", ""),
          ("Cleveland Cavaliers", "Cleveland, Ohio", "Rocket Mortgage FieldHouse", 19432, "#860038", "#041E42", "#FDBB30", ""),
          ("Dallas Mavericks", "Dallas, Texas", "American Airlines Center", 19200, "#00538C", "#002B5E", "#B8C4CA", ""),
          ("Denver Nuggets", "Denver, Colorado", "Ball Arena", 19520, "#0E2240", "#FEC524", "#8B2131", ""),
          ("Detroit Pistons", "Detroit, Michigan", "Little Caesars Arena", 20491, "#C8102E", "#006BB6", "#041E42", ""),
          ("Golden State Warriors", "San Francisco, California", "Chase Center", 18064, "#1D428A", "#FFC72C", "#26282A", ""),
          ("Houston Rockets", "Houston, Texas", "Toyota Center", 18055, "#CE1141", "#000000", "#C4CED4", ""),
          ("Indiana Pacers", "Indianapolis, Indiana", "Gainbridge Fieldhouse", 17923, "#002D62", "#FDBB30", "#BEC0C2", ""),
          ("Los Angeles Clippers", "Los Angeles, California", "Crypto.com Arena", 19068, "#C8102E", "#1D428A", "#000000", ""),
          ("Los Angeles Lakers", "Los Angeles, California", "Crypto.com Arena", 18997, "#552583", "#FDB927", "#000000", ""),
          ("Memphis Grizzlies", "Memphis, Tennessee", "FedExForum", 18119, "#5D76A9", "#12173F", "#707271", ""),
          ("Miami Heat", "Miami, Florida", "Kaseya Center", 19600, "#98002E", "#F9A01B", "#000000", ""),
          ("Milwaukee Bucks", "Milwaukee, Wisconsin", "Fiserv Forum", 17500, "#00471B", "#EEE1C6", "#0077C0", ""),
          ("Minnesota Timberwolves", "Minneapolis, Minnesota", "Target Center", 19356, "#0C2340", "#78BE20", "#236192", ""),
          ("New Orleans Pelicans", "New Orleans, Louisiana", "Smoothie King Center", 16867, "#0C2340", "#C8102E", "#85714D", ""),
          ("New York Knicks", "New York, New York", "Madison Square Garden", 19812, "#006BB6", "#F58426", "#BEC0C2", ""),
          ("Oklahoma City Thunder", "Oklahoma City, Oklahoma", "Paycom Center", 18203, "#007AC1", "#EF3B24", "#002D62", ""),
          ("Orlando Magic", "Orlando, Florida", "Kia Center", 18846, "#0077C0", "#C4CED4", "#000000", ""),
          ("Philadelphia 76ers", "Philadelphia, Pennsylvania", "Wells Fargo Center", 21600, "#006BB6", "#ED174C", "#002B5C", ""),
          ("Phoenix Suns", "Phoenix, Arizona", "Footprint Center", 18055, "#1D1160", "#E56020", "#000000", ""),
          ("Portland Trail Blazers", "Portland, Oregon", "Moda Center", 19441, "#E03A3E", "#000000", "#BAC3C9", ""),
          ("Sacramento Kings", "Sacramento, California", "Golden 1 Center", 17583, "#5A2D81", "#63727A", "#000000", ""),
          ("San Antonio Spurs", "San Antonio, Texas", "Frost Bank Center", 18354, "#C4CED4", "#000000", "#8A8D8F", ""),
          ("Toronto Raptors", "Toronto, Ontario", "Scotiabank Arena", 19800, "#CE1141", "#000000", "#A1A1A4", ""),
          ("Utah Jazz", "Salt Lake City, Utah", "Delta Center", 18306, "#002B5C", "#00471B", "#F9A01B", ""),
          ("Washington Wizards", "Washington, D.C.", "Capital One Arena", 20356, "#002B5C", "#E31837", "#FFFFFF", "")
      };

  }

}
