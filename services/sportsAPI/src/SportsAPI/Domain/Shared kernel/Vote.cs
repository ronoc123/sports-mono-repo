
namespace Domain.User.Entities
{
    public class Vote : Entity<VoteId>
    {
        // Parameterless constructor for EF Core
        internal Vote() { }

        public static Vote Create()
        {
            return new Vote
            {
                Id = VoteId.Of(Guid.NewGuid()),
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
