

namespace Domain.SharedKernal
{
    public class Code : Entity<CodeId>
    {
        public int VotesAwarded { get; set; }
        public bool IsRedeemed { get; set; }
        public DateTime? RedeemedAt { get; set; }
        public UserId? RedeemerId { get; set; }
        public OrganizationId OrganizationId { get; set; }
    }
}
