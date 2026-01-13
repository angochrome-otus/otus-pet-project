using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SteelDesignerEngineer.Domain.Entities;
using SteelDesignerEngineer.Domain.Repositories;
using SteelDesignerEngineer.Domain.Services;

namespace SteelDesignerEngineer.WebApi.Seeders;

/// <summary>
/// Seeder для создания администратора системы
/// </summary>
public static class AdminSeeder
{
    public static async Task SeedAdminUserAsync(IServiceProvider services, ILogger logger)
    {
        try
        {
            var userRepository = services.GetRequiredService<IUserRepository>();
            var passwordHashService = services.GetRequiredService<IPasswordHashService>();

            logger.LogInformation("?? Checking for admin user...");

            // Check if admin already exists
            const string adminEmail = "i@nnshchegolev.ru";
            var existingAdmin = await userRepository.GetByEmailAsync(adminEmail);

            if (existingAdmin != null)
            {
                logger.LogInformation("??  Admin user already exists: {Email}", adminEmail);
                return;
            }

            // Create admin user
            var admin = new Admin
            {
                Email = adminEmail,
                PasswordHash = passwordHashService.HashPassword("xxx711717"),
                FirstName = "Nikita",
                LastName = "Shchegolev",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                AuthProvider = "local",
                Permissions = new List<string>
                {
                    "ManageUsers",
                    "ManageCourses",
                    "ViewReports",
                    "SystemSettings",
                    "DeleteUsers",
                    "ManageRoles"
                },
                Notes = "System Administrator - Full Access"
            };

            await userRepository.CreateAsync(admin);

            logger.LogInformation("? Admin user created successfully");
            logger.LogInformation("   Email: {Email}", adminEmail);
            logger.LogInformation("   Name: {FirstName} {LastName}", admin.FirstName, admin.LastName);
            logger.LogInformation("   Password: xxx711717");
            logger.LogInformation("   Permissions: {Permissions}", string.Join(", ", admin.Permissions));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "? Error seeding admin user");
        }
    }
}
