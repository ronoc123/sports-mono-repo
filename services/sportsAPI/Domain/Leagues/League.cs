
using BuildingBlocks.Exceptions;
using Domain.Organizations;
using Domain.Organizations.Entities;

namespace Domain.Leagues
{
    public class League : Aggregate<LeagueId>
    {
        internal League() { }

        public string Name { get; private set; } = string.Empty;

        public static League Create(string name)
        {
          ArgumentException.ThrowIfNullOrEmpty(name);
          if (name.Length > 200)
            throw new ArgumentException("League name cannot exceed 200 characters.");

          return new League
          {
            Id = LeagueId.Of(Guid.NewGuid()),
            Name = name.Trim(),
            CreatedAt = DateTime.UtcNow
          };
        }

        /// <summary>Rename this league, enforcing invariants.</summary>
        public void Rename(string name)
        {
          ArgumentException.ThrowIfNullOrEmpty(name);

          var trimmed = name.Trim();
          if (trimmed.Length > 200)
            throw new DomainException("League name cannot exceed 200 characters.");

          if (string.Equals(Name, trimmed, StringComparison.OrdinalIgnoreCase))
            return; // no change

          Name = trimmed;
          // AddEvent(new LeagueRenamed(Id, Name));
        }

      }

      // Domain events (keep if you publish them)
      public sealed record LeagueCreated(LeagueId LeagueId, string Name);
      public sealed record LeagueRenamed(LeagueId LeagueId, string Name);
}
