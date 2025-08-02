namespace sportsAPI.DTO.Codes
{
    public class RedeemCodeRequest
    {
        public Guid CodeId { get; set; }
        public Guid OrganziationId { get; set; }
        public Guid RedeemerId { get; set; }
    }
}
