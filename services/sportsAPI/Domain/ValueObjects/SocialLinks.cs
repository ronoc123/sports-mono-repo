

namespace Domain.ValueObjects
{
    public class SocialLinks
    {
        // EF Core compatible properties with private setters
        public string? Website { get; private set; }
        public string? Facebook { get; private set; }
        public string? Twitter { get; private set; }
        public string? Instagram { get; private set; }
        public string? YoutubeUrl { get; private set; }

        // Parameterless constructor for EF Core
        private SocialLinks() { }

        // Public constructor for domain use
        public SocialLinks(string? website, string? facebook, string? twitter, string? instagram)
        {
            Website = website;
            Facebook = facebook;
            Twitter = twitter;
            Instagram = instagram;
        }

        public static SocialLinks Of(string? website, string? facebook, string? twitter, string? instagram, string? youtubeUrl = null)
        {
            var socialLinks = new SocialLinks(website, facebook, twitter, instagram);
            socialLinks.YoutubeUrl = youtubeUrl;
            return socialLinks;
        }

        // Equality members for value object behavior
        public override bool Equals(object? obj)
        {
            return obj is SocialLinks links &&
                   Website == links.Website &&
                   Facebook == links.Facebook &&
                   Twitter == links.Twitter &&
                   Instagram == links.Instagram &&
                   YoutubeUrl == links.YoutubeUrl;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Website, Facebook, Twitter, Instagram, YoutubeUrl);
        }

        public static bool operator ==(SocialLinks? left, SocialLinks? right)
        {
            return EqualityComparer<SocialLinks>.Default.Equals(left, right);
        }

        public static bool operator !=(SocialLinks? left, SocialLinks? right)
        {
            return !(left == right);
        }
    }
}
