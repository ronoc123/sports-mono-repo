namespace sportsAPI.DTO.Organization_requests
{
    public class AddOrganizationResponse
    {
        public Guid OrganizationId { get; set; }
        public string Name { get; set; }
        public Guid LeagueId { get; set; }
        public string LeagueName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
