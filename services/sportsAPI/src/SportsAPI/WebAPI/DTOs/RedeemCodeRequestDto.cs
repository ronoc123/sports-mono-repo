using Domain.ValueObjects.ConcreteTypes;

namespace sportsAPI.DTO;

public class RedeemCodeRequestDto
{
    public CodeId CodeId { get; set; }
    public UserId UserId { get; set; }
}
