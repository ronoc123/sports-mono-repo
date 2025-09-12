

namespace Domain.Users
{
    public class User : Aggregate<UserId>
    {
        internal User()
        {
        }

        public string Email { get; private set; } = string.Empty;
        public string UserName { get; private set; } = string.Empty;
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string? Phone { get; private set; }
        public DateTime? DateOfBirth { get; private set; }
        public string? Bio { get; private set; }
        public string? Avatar { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        public static User Create(Guid id, string email, string userName, string firstName, string lastName)
        {
            return new User
            {
                Id = UserId.Of(id),
                Email = email,
                UserName = userName,
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void UpdateEmail(string email)
        {
            ArgumentException.ThrowIfNullOrEmpty(email);
            Email = email;
        }

        public void UpdateUserName(string userName)
        {
            ArgumentException.ThrowIfNullOrEmpty(userName);
            UserName = userName;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateProfile(string firstName, string lastName, string? phone = null,
            DateTime? dateOfBirth = null, string? bio = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(firstName);
            ArgumentException.ThrowIfNullOrEmpty(lastName);

            FirstName = firstName;
            LastName = lastName;
            Phone = phone;
            DateOfBirth = dateOfBirth;
            Bio = bio;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateAvatar(string avatarUrl)
        {
            Avatar = avatarUrl;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
