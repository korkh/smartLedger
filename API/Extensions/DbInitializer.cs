using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Storage;

namespace API.Extensions
{
    public static class DbInitializer
    {
        public static async Task InitDb(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                var context = services.GetRequiredService<DataContext>();
                var userManager = services.GetRequiredService<UserManager<User>>();

                // Apply pending migrations
                // This is crucial since we changed IDs from int to Guid
                await context.Database.MigrateAsync();

                // Seed initial data
                await Seed.SeedData(context, userManager);

                logger.LogInformation("Database migration and seeding completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database migration or seeding");
                throw; // Re-throw to stop the app if DB is not ready
            }
        }
    }
}
