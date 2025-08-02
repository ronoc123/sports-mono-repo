

namespace Domain.ValueObjects
{
    public class Venue
    {
        // EF Core compatible properties with private setters
        public string Stadium { get; private set; } = string.Empty;
        public string Location { get; private set; } = string.Empty;
        public int Capacity { get; private set; }

        // Legacy properties for backward compatibility
        public int StadiumCapacity => Capacity;

        // Parameterless constructor for EF Core
        private Venue() { }

        // Public constructor for domain use
        public Venue(string stadium, string location, int capacity)
        {
            Stadium = stadium ?? string.Empty;
            Location = location ?? string.Empty;
            Capacity = capacity;
        }

        public static Venue Of(string stadium, string location, int stadiumCapacity)
        {
            if (string.IsNullOrWhiteSpace(stadium))
            {
                throw new ArgumentException("Stadium is required.", nameof(stadium));
            }

            if (stadiumCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stadiumCapacity), "Stadium capacity must be greater than zero.");
            }

            return new Venue(stadium, location ?? string.Empty, stadiumCapacity);
        }

        // Equality members for value object behavior
        public override bool Equals(object? obj)
        {
            return obj is Venue venue &&
                   Stadium == venue.Stadium &&
                   Location == venue.Location &&
                   Capacity == venue.Capacity;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Stadium, Location, Capacity);
        }

        public static bool operator ==(Venue? left, Venue? right)
        {
            return EqualityComparer<Venue>.Default.Equals(left, right);
        }

        public static bool operator !=(Venue? left, Venue? right)
        {
            return !(left == right);
        }
    }
}
