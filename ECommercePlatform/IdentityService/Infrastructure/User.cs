using Microsoft.AspNetCore.Identity;

namespace IdentityService.Infrastructure
{
    public class User : IdentityUser<Guid>
    {
        public bool IsActive { get; set; } = true;
    }
}
