using Microsoft.AspNetCore.Identity;

namespace IdentityService.Infrastructure
{
    public class Role : IdentityRole<Guid>
    {
        public string Description { get; set; } = default!;
    }
}
