

namespace Domain.ValueObjects
{
    public class TeamColors
    {
        // EF Core compatible properties with private setters
        public string Primary { get; private set; } = string.Empty;
        public string Secondary { get; private set; } = string.Empty;
        public string Tertiary { get; private set; } = string.Empty;

        // Legacy properties for backward compatibility
        public string Color1 => Primary;
        public string Color2 => Secondary;
        public string Color3 => Tertiary;

        // Parameterless constructor for EF Core
        private TeamColors() { }

        // Public constructor for domain use
        public TeamColors(string primary, string secondary, string tertiary)
        {
            Primary = primary ?? string.Empty;
            Secondary = secondary ?? string.Empty;
            Tertiary = tertiary ?? string.Empty;
        }

        public static TeamColors Of(string color1, string color2, string color3)
        {
            if (string.IsNullOrWhiteSpace(color1)) throw new ArgumentException("Color1 is required.");
            if (string.IsNullOrWhiteSpace(color2)) throw new ArgumentException("Color2 is required.");
            if (string.IsNullOrWhiteSpace(color3)) throw new ArgumentException("Color3 is required.");

            return new TeamColors(color1, color2, color3);
        }

        // ✅ Default green shades
        public static TeamColors Default => new TeamColors(
            "#A8D5BA",
            "#6FBF8F",
            "#3A8D61"
        );

        // Equality members for value object behavior
        public override bool Equals(object? obj)
        {
            return obj is TeamColors colors &&
                   Primary == colors.Primary &&
                   Secondary == colors.Secondary &&
                   Tertiary == colors.Tertiary;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Primary, Secondary, Tertiary);
        }

        public static bool operator ==(TeamColors? left, TeamColors? right)
        {
            return EqualityComparer<TeamColors>.Default.Equals(left, right);
        }

        public static bool operator !=(TeamColors? left, TeamColors? right)
        {
            return !(left == right);
        }
    }
}
