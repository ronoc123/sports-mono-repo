/*
// TODO: Update DatabaseSeeder for new Clean Architecture
using Microsoft.EntityFrameworkCore;
using sportsAPI.model;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace sportsAPI.Data
{
    public class DatabaseSeeder
    {
        private readonly AppDbContext _appDbContext;
        private readonly HttpClient _httpClient;

        public DatabaseSeeder(AppDbContext appDbContext, HttpClient httpClient)
        {
            _appDbContext = appDbContext;
            _httpClient = httpClient;
        }

        public async Task SeedNFLDataAsync()
        {
            const string leagueName = "NFL";

            // Check if the league already exists
            var existingLeague = await _appDbContext.Leagues.FirstOrDefaultAsync(l => l.Name == leagueName);
            if (existingLeague != null)
            {
                Console.WriteLine($"League {leagueName} already exists. Skipping seed.");
                return;
            }

            // Fetch data from SportsDB API
            const string sportsDbUrl = "https://www.thesportsdb.com/api/v1/json/3/search_all_teams.php?l=NFL";
            var response = await _httpClient.GetAsync(sportsDbUrl);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Failed to fetch data from SportsDB API.");
                return;
            }

            // Read the response content as a string
            var jsonResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine("API Response Content: " + jsonResponse); // Log the content for debugging

            try
            {
                // Deserialize JSON dynamically
                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(jsonResponse);

                if (apiResponse?.Teams == null)
                {
                    Console.WriteLine("No teams found in the API response.");
                    return;
                }

                // Create the league
                var league = new League
                {
                    Id = Guid.NewGuid(),
                    Name = leagueName,
                };

                // Add the League to the database
                await _appDbContext.Leagues.AddAsync(league);
                await _appDbContext.SaveChangesAsync();


                var samplePlayers = new List<TempPlayer>
                {
                    // Arizona Cardinals
                    new TempPlayer { Name = "Kyler Murray", Position = "Quarterback", Age = 26, ImageUrl = "https://example.com/kyler_murray.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Arizona Cardinals" },
                    new TempPlayer { Name = "DeAndre Hopkins", Position = "Wide Receiver", Age = 31, ImageUrl = "https://example.com/deandre_hopkins.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Arizona Cardinals" },
                    new TempPlayer { Name = "Budda Baker", Position = "Safety", Age = 28, ImageUrl = "https://example.com/budda_baker.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Arizona Cardinals" },

                    // Dallas Cowboys
                    new TempPlayer { Name = "Dak Prescott", Position = "Quarterback", Age = 31, ImageUrl = "https://example.com/dak_prescott.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Dallas Cowboys" },
                    new TempPlayer { Name = "CeeDee Lamb", Position = "Wide Receiver", Age = 24, ImageUrl = "https://example.com/ceedee_lamb.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Dallas Cowboys" },
                    new TempPlayer { Name = "Micah Parsons", Position = "Linebacker", Age = 24, ImageUrl = "https://example.com/micah_parsons.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Dallas Cowboys" },
                    new TempPlayer { Name = "Ezekiel Elliott", Position = "Running Back", Age = 28, ImageUrl = "https://example.com/ezekiel_elliott.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Dallas Cowboys" },
                    new TempPlayer { Name = "Tony Pollard", Position = "Running Back", Age = 26, ImageUrl = "https://example.com/tony_pollard.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Dallas Cowboys" },
                    new TempPlayer { Name = "Tyron Smith", Position = "Offensive Tackle", Age = 33, ImageUrl = "https://example.com/tyron_smith.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Dallas Cowboys" },
                    new TempPlayer { Name = "Amari Cooper", Position = "Wide Receiver", Age = 29, ImageUrl = "https://example.com/amari_cooper.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Dallas Cowboys" },
                    new TempPlayer { Name = "Trevon Diggs", Position = "Cornerback", Age = 26, ImageUrl = "https://example.com/trevon_diggs.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Dallas Cowboys" },
                    new TempPlayer { Name = "Leighton Vander Esch", Position = "Linebacker", Age = 27, ImageUrl = "https://example.com/leighton_vander_esch.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Dallas Cowboys" },
                    new TempPlayer { Name = "Blake Jarwin", Position = "Tight End", Age = 28, ImageUrl = "https://example.com/blake_jarwin.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Dallas Cowboys" },
                    new TempPlayer { Name = "Dakota Dozier", Position = "Offensive Guard", Age = 31, ImageUrl = "https://example.com/dakota_dozier.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Dallas Cowboys" },
                    new TempPlayer { Name = "Micah Hyde", Position = "Safety", Age = 33, ImageUrl = "https://example.com/micah_hyde.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Dallas Cowboys" },
                    new TempPlayer { Name = "Dakota Prescott", Position = "Quarterback", Age = 33, ImageUrl = "https://example.com/dakota_prescott.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Dallas Cowboys" },
                    new TempPlayer { Name = "Tyler Smith", Position = "Offensive Tackle", Age = 24, ImageUrl = "https://example.com/tyler_smith.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Dallas Cowboys" },

                    // Denver Broncos
                    new TempPlayer { Name = "Russell Wilson", Position = "Quarterback", Age = 34, ImageUrl = "https://example.com/russell_wilson.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Denver Broncos" },
                    new TempPlayer { Name = "Jerry Jeudy", Position = "Wide Receiver", Age = 24, ImageUrl = "https://example.com/jerry_jeudy.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Denver Broncos" },
                    new TempPlayer { Name = "Patrick Surtain II", Position = "Cornerback", Age = 24, ImageUrl = "https://example.com/patrick_surtain.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Denver Broncos" },

                    // Detroit Lions
                    new TempPlayer { Name = "Jared Goff", Position = "Quarterback", Age = 29, ImageUrl = "https://example.com/jared_goff.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Detroit Lions" },
                    new TempPlayer { Name = "Amon-Ra St. Brown", Position = "Wide Receiver", Age = 24, ImageUrl = "https://example.com/amonra_st_brown.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Detroit Lions" },
                    new TempPlayer { Name = "Hutchinson Aidan", Position = "Defensive End", Age = 24, ImageUrl = "https://example.com/hutchinson_aidan.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Detroit Lions" },

                     // Atlanta Falcons
                    new TempPlayer { Name = "Desmond Ridder", Position = "Quarterback", Age = 24, ImageUrl = "https://example.com/desmond_ridder.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Atlanta Falcons" },
                    new TempPlayer { Name = "Kyle Pitts", Position = "Tight End", Age = 23, ImageUrl = "https://example.com/kyle_pitts.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Atlanta Falcons" },
                    new TempPlayer { Name = "AJ Terrell", Position = "Cornerback", Age = 25, ImageUrl = "https://example.com/aj_terrell.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Atlanta Falcons" },

                    // Baltimore Ravens
                    new TempPlayer { Name = "Lamar Jackson", Position = "Quarterback", Age = 26, ImageUrl = "https://example.com/lamar_jackson.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Baltimore Ravens" },
                    new TempPlayer { Name = "Mark Andrews", Position = "Tight End", Age = 28, ImageUrl = "https://example.com/mark_andrews.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Baltimore Ravens" },
                    new TempPlayer { Name = "Marlon Humphrey", Position = "Cornerback", Age = 27, ImageUrl = "https://example.com/marlon_humphrey.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Baltimore Ravens" },

                    // Buffalo Bills
                    new TempPlayer { Name = "Josh Allen", Position = "Quarterback", Age = 27, ImageUrl = "https://example.com/josh_allen.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Buffalo Bills" },
                    new TempPlayer { Name = "Stefon Diggs", Position = "Wide Receiver", Age = 30, ImageUrl = "https://example.com/stefon_diggs.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Buffalo Bills" },
                    new TempPlayer { Name = "Micah Hyde", Position = "Safety", Age = 33, ImageUrl = "https://example.com/micah_hyde.jpg", UpdatedAt = DateTime.UtcNow, TeamName = "Buffalo Bills" },
                };

                // Map each team to an Organization
                foreach (var team in apiResponse.Teams)
                {
                    var organization = new Organization
                    {
                        Id = Guid.NewGuid(),
                        Name = team.StrTeam,
                        TeamId = team.IdTeam,
                        TeamName = team.StrTeam,
                        TeamShortName = team.StrTeamShort,
                        FormedYear = int.TryParse(team.IntFormedYear, out var year) ? year : 0,
                        Sport = team.StrSport,
                        Stadium = team.StrStadium,
                        Location = team.StrLocation,
                        StadiumCapacity = int.TryParse(team.IntStadiumCapacity, out var capacity) ? capacity : 0,
                        Website = team.StrWebsite,
                        Facebook = team.StrFacebook,
                        Twitter = team.StrTwitter,
                        Instagram = team.StrInstagram,
                        Description = team.StrDescriptionEN,
                        Color1 = team.StrColour1,
                        Color2 = team.StrColour2,
                        Color3 = team.StrColour3,
                        BadgeUrl = team.StrBadge,
                        LogoUrl = team.StrLogo,
                        Fanart1Url = team.StrFanart1,
                        Fanart2Url = team.StrFanart2,
                        Fanart3Url = team.StrFanart3,
                        Fanart4Url = team.StrFanart4,
                        BannerUrl = team.StrBanner,
                        YoutubeUrl = team.StrYoutube,
                        IsLocked = false,
                        CreatedAt = DateTime.UtcNow,
                        League = league
                    };

                    // Add league and organizations to the database
                    await _appDbContext.Organizations.AddAsync(organization);

                    var primaryColor = team.StrColour1 ?? "#000000"; // Default to black if null
                    var secondaryColor = AdjustBrightness(primaryColor, 0.7);
                    var tertiaryColor = AdjustBrightness(primaryColor, 0.85);

                    var theme = new Theme
                    {
                        Id = Guid.NewGuid(),
                        Name = $"{team.StrTeam}",
                        ColorPrimary = primaryColor,
                        ColorSecondary = secondaryColor,
                        ColorTertiary = tertiaryColor,
                        Logo = $"{team.StrTeam.ToLower().Replace(" ", "-")}.png"
                    };

                    await _appDbContext.Themes.AddAsync(theme);

                    foreach (var samplePlayer in samplePlayers)
                    {
                        if (samplePlayer.TeamName.Equals(organization.TeamName))
                        {
                            var player = new Player
                            {
                                Id = Guid.NewGuid(),
                                Name = samplePlayer.Name,
                                Position = samplePlayer.Position,
                                //TeamName = organization.TeamName,
                                ImageUrl = samplePlayer.ImageUrl,
                                Age = samplePlayer.Age,
                                UpdatedAt = DateTime.UtcNow,
                                organization = organization
                            };

                            await _appDbContext.Players.AddAsync(player);
                        }
                    }
                    await _appDbContext.SaveChangesAsync();

                }
                var orgs = await _appDbContext.Organizations.ToArrayAsync();

                var currentPlayers = await _appDbContext.Players.ToArrayAsync();
                foreach (var o in orgs)
                {
                    foreach (var p in currentPlayers)
                    {

                        var playerOption = new PlayerOption
                        {
                            Id = Guid.NewGuid(),
                            Title = $"Vote for {p.Name}",
                            Description = $"{p.Name} is a key player for {o.Name}. Cast your vote!",
                            Votes = 0,
                            CreatedAt = DateTime.UtcNow,
                            ExpiresAt = DateTime.UtcNow.AddMonths(6),
                            Player = p,
                            Organization = o
                        };
                        await _appDbContext.PlayerOptions.AddAsync(playerOption);
                    }
                }
                await _appDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while processing API response: " + ex.Message);
            }
        }

        private string AdjustBrightness(string hexColor, double factor)
        {
            if (string.IsNullOrEmpty(hexColor) || !hexColor.StartsWith("#") || hexColor.Length != 7)
            {
                return "#000000"; // Default to black for invalid input
            }

            var r = Convert.ToInt32(hexColor.Substring(1, 2), 16);
            var g = Convert.ToInt32(hexColor.Substring(3, 2), 16);
            var b = Convert.ToInt32(hexColor.Substring(5, 2), 16);

            r = Math.Min((int)(r * factor), 255);
            g = Math.Min((int)(g * factor), 255);
            b = Math.Min((int)(b * factor), 255);

            return $"#{r:X2}{g:X2}{b:X2}";
        }
        public class Team
        {
            [JsonPropertyName("idTeam")]
            public string IdTeam { get; set; }

            [JsonPropertyName("strTeam")]
            public string StrTeam { get; set; }

            [JsonPropertyName("strTeamShort")]
            public string StrTeamShort { get; set; }

            [JsonPropertyName("intFormedYear")]
            public string IntFormedYear { get; set; }

            [JsonPropertyName("strSport")]
            public string StrSport { get; set; }

            [JsonPropertyName("strLeague")]
            public string StrLeague { get; set; }

            [JsonPropertyName("strStadium")]
            public string StrStadium { get; set; }

            [JsonPropertyName("strLocation")]
            public string StrLocation { get; set; }

            [JsonPropertyName("intStadiumCapacity")]
            public string IntStadiumCapacity { get; set; }

            [JsonPropertyName("strWebsite")]
            public string StrWebsite { get; set; }

            [JsonPropertyName("strFacebook")]
            public string StrFacebook { get; set; }

            [JsonPropertyName("strTwitter")]
            public string StrTwitter { get; set; }

            [JsonPropertyName("strInstagram")]
            public string StrInstagram { get; set; }

            [JsonPropertyName("strDescriptionEN")]
            public string StrDescriptionEN { get; set; }

            [JsonPropertyName("strColour1")]
            public string StrColour1 { get; set; }

            [JsonPropertyName("strColour2")]
            public string StrColour2 { get; set; }

            [JsonPropertyName("strColour3")]
            public string StrColour3 { get; set; }

            [JsonPropertyName("strBadge")]
            public string StrBadge { get; set; }

            [JsonPropertyName("strLogo")]
            public string StrLogo { get; set; }

            [JsonPropertyName("strFanart1")]
            public string StrFanart1 { get; set; }

            [JsonPropertyName("strFanart2")]
            public string StrFanart2 { get; set; }

            [JsonPropertyName("strFanart3")]
            public string StrFanart3 { get; set; }

            [JsonPropertyName("strFanart4")]
            public string StrFanart4 { get; set; }

            [JsonPropertyName("strBanner")]
            public string StrBanner { get; set; }

            [JsonPropertyName("strYoutube")]
            public string StrYoutube { get; set; }
        }


        public class TempPlayer
        {
            public string Name { get; set; }
            public string Position { get; set; }
            public int Age { get; set; }
            public string ImageUrl { get; set; }
            public DateTime UpdatedAt { get; set; }
            public string TeamName { get; set; }  // New field for TeamName
        }

        public class ApiResponse
    {
        [JsonPropertyName("teams")]
        public List<Team> Teams { get; set; }
    }

}
}
*/
