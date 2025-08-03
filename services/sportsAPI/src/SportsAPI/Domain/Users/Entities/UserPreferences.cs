namespace Domain.Users.Entities
{
    public class UserPreferences
    {
        public bool EmailNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = false;
        public string Theme { get; set; } = "auto"; // light, dark, auto
        public string Language { get; set; } = "en";
        public string Timezone { get; set; } = "UTC";
        public PrivacySettings Privacy { get; set; } = new();

        public UserPreferences() { }

        public UserPreferences(bool emailNotifications, bool pushNotifications, 
            string theme, string language, string timezone, PrivacySettings privacy)
        {
            EmailNotifications = emailNotifications;
            PushNotifications = pushNotifications;
            Theme = theme;
            Language = language;
            Timezone = timezone;
            Privacy = privacy;
        }
    }

    public class PrivacySettings
    {
        public string ProfileVisibility { get; set; } = "public"; // public, private, friends
        public bool ShowEmail { get; set; } = false;
        public bool ShowPhone { get; set; } = false;

        public PrivacySettings() { }

        public PrivacySettings(string profileVisibility, bool showEmail, bool showPhone)
        {
            ProfileVisibility = profileVisibility;
            ShowEmail = showEmail;
            ShowPhone = showPhone;
        }
    }
}
