using ECommercePlatform.Domain.Abstractions;

using IdentityService.Domain.ValueObjects;

namespace IdentityService.Domain.Aggregates
{
    public class User : AggregateRoot
    {
        public Email Email { get; private set; }
        public string PasswordHash { get; private set; }
        public UserRole Role { get; private set; }
        public bool IsEmailConfirmed { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private User()
        {
            Email = default!;
            PasswordHash = default!;
        }

        public User Register(string email, string passwordHash)
        {
            User user = new User
            {
                Id = Guid.NewGuid(),
                Email = new Email(email),
                PasswordHash = passwordHash,
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            return user;
        }

        public void ConfirmEmail()
        {
            IsEmailConfirmed = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void ChangeRole(UserRole newRole)
        {
            Role = newRole;
        }
    }
}
