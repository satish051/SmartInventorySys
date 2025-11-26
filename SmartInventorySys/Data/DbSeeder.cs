using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection; // Needed for GetRequiredService
using System;
using System.Threading.Tasks;

namespace SmartInventorySys.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            // Use GetRequiredService to prevent null warning
            var userManager = service.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();

            await CreateRoleAsync(roleManager, "Admin");
            await CreateRoleAsync(roleManager, "Manager");
            await CreateRoleAsync(roleManager, "Staff");

            var adminEmail = "admin@inventory.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var user = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }

        private static async Task CreateRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}