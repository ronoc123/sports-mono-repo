namespace sportsAPI.DTO.Codes
{
    public class RedeemCodeResponse
    {
        public Guid CodeId { get; set; }
        public bool IsRedeemed { get; set; }
        public DateTime? RedeemedAt { get; set; }
    }
}
