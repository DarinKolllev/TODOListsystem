using Microsoft.AspNetCore.Identity;
using TODOlistsystem.Models;

namespace TODOlistsystem.Data {
    public static class DbInitializer {
        public static async Task SeedAdminAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager) {
            // Create Roles
            string[] roles = { "Admin", "User" };

            foreach (var role in roles) {
                if (!await roleManager.RoleExistsAsync(role)) {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create Default Admin
            string adminEmail = "admin@todolist.com";
            string adminPassword = "Admin@123";

            if (await userManager.FindByEmailAsync(adminEmail) == null) {
                var adminUser = new ApplicationUser {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded) {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
