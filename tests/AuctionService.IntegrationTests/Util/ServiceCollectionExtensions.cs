using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests.Util;
public static class ServiceCollectionExtensions
{
    public static void RemoveDbContext<T>(this IServiceCollection services)
    {
        // 2.1.1. Remove current DB Context.
        ServiceDescriptor descriptor = services
                        .SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

        if (descriptor != null)
            services.Remove(descriptor);
    }
    public static void EnsureCreated<T>(this IServiceCollection services)
    {
        // 2.1.3. Migrate database schema.
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();

        // 2.1.4. Seed test data.
        DbHelper.InitDbForTests(db);
    }
}
