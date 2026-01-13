using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SteelDesignerEngineer.Application.Interfaces;
using SteelDesignerEngineer.Domain.Entities;

namespace SteelDesignerEngineer.WebApi.Seeders;

/// <summary>
/// Seeder для загрузки всех HTML страниц из wwwroot в MongoDB
/// </summary>
public static class PagesSeeder
{
    public static async Task SeedAllPagesAsync(IServiceProvider services, ILogger logger, string wwwrootPath)
    {
        try
        {
            logger.LogInformation("?? Starting pages seeding...");
            
            var pageContentService = services.GetRequiredService<IPageContentApplicationService>();

            // Список всех HTML страниц для загрузки в MongoDB
            var pagesToSeed = new[]
            {
                new { FileName = "index.html", PageName = "home", Title = "Home - Steel Designer Engineer" },
                new { FileName = "login.html", PageName = "login", Title = "Login - Steel Designer Engineer" },
                new { FileName = "register.html", PageName = "register", Title = "Register - Steel Designer Engineer" },
                new { FileName = "profile.html", PageName = "profile", Title = "Profile - Steel Designer Engineer" },
                new { FileName = "student-dashboard.html", PageName = "student-dashboard", Title = "Student Dashboard" },
                new { FileName = "teacher-dashboard.html", PageName = "teacher-dashboard", Title = "Teacher Dashboard" },
                new { FileName = "admin-dashboard.html", PageName = "admin-dashboard", Title = "Admin Dashboard" },
                new { FileName = "oauth-callback.html", PageName = "oauth-callback", Title = "OAuth Callback" }
            };

            int created = 0;
            int updated = 0;
            int skipped = 0;

            foreach (var pageInfo in pagesToSeed)
            {
                try
                {
                    var filePath = Path.Combine(wwwrootPath, pageInfo.FileName);
                    
                    if (!File.Exists(filePath))
                    {
                        logger.LogWarning("??  File not found: {FileName}", pageInfo.FileName);
                        skipped++;
                        continue;
                    }

                    var html = await File.ReadAllTextAsync(filePath);
                    
                    var existingPage = await pageContentService.GetPageContentByPageNameAsync(pageInfo.PageName);
                    
                    if (existingPage == null)
                    {
                        // Create new page
                        var newPage = new PageContent
                        {
                            PageName = pageInfo.PageName,
                            Title = pageInfo.Title,
                            Content = html,
                            LastModified = DateTime.UtcNow
                        };
                        
                        await pageContentService.CreatePageContentAsync(newPage);
                        logger.LogInformation("? Created: {PageName} ({FileName})", pageInfo.PageName, pageInfo.FileName);
                        created++;
                    }
                    else
                    {
                        // Update existing page
                        existingPage.Content = html;
                        existingPage.Title = pageInfo.Title;
                        existingPage.LastModified = DateTime.UtcNow;
                        
                        await pageContentService.UpdatePageContentAsync(existingPage.Id, existingPage);
                        logger.LogInformation("?? Updated: {PageName} ({FileName})", pageInfo.PageName, pageInfo.FileName);
                        updated++;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "? Error seeding page: {FileName}", pageInfo.FileName);
                    skipped++;
                }
            }

            logger.LogInformation("=".PadRight(60, '='));
            logger.LogInformation("?? Pages Seeding Summary:");
            logger.LogInformation("   ? Created: {Created}", created);
            logger.LogInformation("   ?? Updated: {Updated}", updated);
            logger.LogInformation("   ??  Skipped: {Skipped}", skipped);
            logger.LogInformation("   ?? Total processed: {Total}", created + updated + skipped);
            logger.LogInformation("=".PadRight(60, '='));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "? Critical error during pages seeding");
        }
    }
}
