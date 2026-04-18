using ECommerce.Domain.Contracts;
using ECommerce.Persistence.Data.DbContexts;
using ECommerce.Persistence.IdentityData.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Extensions
{
    public static class WebApplicationRegister
    {
        public static async Task<WebApplication> MigrateDataBaseAsync(this WebApplication app)
        {
            await using var scope = app.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext>();

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                dbContext.Database.Migrate();
            }

            return app;
        }

        public static async Task<WebApplication> MigratIdentityeDataBaseAsync(
            this WebApplication app
        )
        {
            await using var scope = app.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<StoreIdentityDbContext>();

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                dbContext.Database.Migrate();
            }

            return app;
        }

        public static async Task<WebApplication> SeedDataAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var dataIntializer = scope.ServiceProvider.GetRequiredKeyedService<IDataIntializer>(
                "Default"
            );

            await dataIntializer.IntializeAsync();

            return app;
        }

        public static async Task<WebApplication> SeedIdentityDataAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var dataIntializer = scope.ServiceProvider.GetRequiredKeyedService<IDataIntializer>(
                "Identity"
            );

            await dataIntializer.IntializeAsync();

            return app;
        }
    }
}
