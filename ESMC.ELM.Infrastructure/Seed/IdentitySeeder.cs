using ESMC.ELM.Domain.Constants;
using ESMC.ELM.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ESMC.ELM.Infrastructure.Seed
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider
                .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            string[] roles =
            {
                AppRoles.SystemAdmin,
                AppRoles.RnDManager,
                AppRoles.ProjectManager,
                AppRoles.Engineer,
                AppRoles.Tester,
                AppRoles.ProductionEngineer,
                AppRoles.AfterSalesEngineer,
                AppRoles.Viewer
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole<Guid>(role));
                }
            }
        }

        public static async Task SeedSystemAdminAsync(
            IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            var adminEmail = "ibrahim.mansour2110@gmail.com";

            var user = await userManager.FindByEmailAsync(adminEmail);

            if (user is not null &&
                !await userManager.IsInRoleAsync(user, AppRoles.SystemAdmin))
            {
                await userManager.AddToRoleAsync(
                    user,
                    AppRoles.SystemAdmin);
            }
        }
    }
}