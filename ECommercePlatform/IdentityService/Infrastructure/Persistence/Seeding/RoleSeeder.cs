using ECommercePlatform.Identity;

using Microsoft.AspNetCore.Identity;

namespace IdentityService.Infrastructure.Persistence.Seeding
{
    public static class RoleSeeder
    {
        public static readonly List<Role> RolesToSeed = new()
        {
            new Role
            {
                Name = Roles.Admin,
                NormalizedName =  Roles.Admin.ToUpperInvariant(),
                Description = "Administrator with full access"
            },
            new Role
            {
                Name = Roles.Customer,
                NormalizedName = Roles.Customer.ToUpperInvariant(),
                Description = "Customer with limited access"
            }
        };

        public static async Task SeedRolesAsync(RoleManager<Role> roleManager)
        {
            foreach (var role in RolesToSeed)
            {
                if (!await roleManager.RoleExistsAsync(role.Name!))
                {
                    IdentityResult result = await roleManager.CreateAsync(role);
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to create role {role.Name}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }
    }
}
