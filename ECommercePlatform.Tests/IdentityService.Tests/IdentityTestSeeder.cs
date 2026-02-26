using IdentityService.Infrastructure;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Tests
{
    public static class IdentityTestSeeder
    {
        public const string Email = "admin@test.com";
        public const string Password = "Admin123!";
        public const string Role = "Admin";

        public static async Task SeedAdminAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<Role>>();

            var user = await userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                user = new User
                {
                    UserName = Email,
                    Email = Email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, Password);
                if (!createResult.Succeeded)
                    throw new Exception("Failed to create Admin user");
            }

            if (!await userManager.IsInRoleAsync(user, Role))
            {
                var roleResult = await userManager.AddToRoleAsync(user, Role);
                if (!roleResult.Succeeded)
                    throw new Exception("Failed to assign Admin role");
            }
        }
    }
}
