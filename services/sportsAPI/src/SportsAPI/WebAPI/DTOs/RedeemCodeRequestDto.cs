namespace sportsAPI.DTO;

public class RedeemCodeRequestDto
{
    public Guid CodeId { get; set; }
    public Guid UserId { get; set; }
}
