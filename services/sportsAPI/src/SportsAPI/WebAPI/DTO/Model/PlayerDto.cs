namespace sportsAPI.DTO.Model
{
    public class PlayerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string TeamName { get; set; }
        public string ImageUrl { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Age { get; set; }
    }
}
