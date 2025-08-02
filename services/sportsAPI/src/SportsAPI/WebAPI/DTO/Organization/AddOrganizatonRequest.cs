namespace sportsAPI.DTO.Organization_requests
{
    public class AddOrganizatonRequest
    {
        public string Name { get; set; }
        public Guid LeagueId { get; set; }

        public string? TeamId { get; set; }
        public string? TeamName { get; set; }
        public string? TeamShortName { get; set; }
        public int? FormedYear { get; set; }
        public string? Sport { get; set; }
        public string? League { get; set; }
        public string? Stadium { get; set; }
        public string? Location { get; set; }
        public int? StadiumCapacity { get; set; }
        public string? Website { get; set; }
        public string? Facebook { get; set; }
        public string? Twitter { get; set; }
        public string? Instagram { get; set; }
        public string? Description { get; set; }
        public string? Color1 { get; set; }
        public string? Color2 { get; set; }
        public string? Color3 { get; set; }
        public string? BadgeUrl { get; set; }
        public string? LogoUrl { get; set; }
        public string? Fanart1Url { get; set; }
        public string? Fanart2Url { get; set; }
        public string? Fanart3Url { get; set; }
        public string? Fanart4Url { get; set; }
        public string? BannerUrl { get; set; }
        public string? EquipmentUrl { get; set; }
        public string? YoutubeUrl { get; set; }
        public bool? IsLocked { get; set; }
    }
}
