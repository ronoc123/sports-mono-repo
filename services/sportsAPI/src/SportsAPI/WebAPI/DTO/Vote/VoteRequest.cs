namespace sportsAPI.DTO.Vote
{
    public class VoteRequest
    {
        public Guid PlayerOptionId { get; set; }

        public Guid VoterId { get; set; }

        public int Votes { get; set; }
    }
}
