using AuctionService.Entities;
using AuctionService.IntegrationTests.Util;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using WebMotions.Fake.Authentication.JwtBearer;

namespace AuctionService.IntegrationTests.Fixtures;
public class CustomWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    /* 
    * We want to start database (PostgreSQL) container when the integration tests start.
    * and dispose of it when the tests finish.
    * We achieve this by using XUnit's IASyncLifetime.
    */

    // 1. Instantiate Database container.
    private PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    // Actions to take when this web application is initialized.
    public async Task InitializeAsync()
    {
        await this._postgreSqlContainer.StartAsync(); // Starts running instance of our test container database.

    }
    // Actions to take when the tests have finished and this web application is disposed of.

    Task IAsyncLifetime.DisposeAsync() => this._postgreSqlContainer.DisposeAsync().AsTask(); // Disposes our test container database.

    // 2. We need to create a fake DB Context, ervice Bus and Identity Server.
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        // Configure fake services here...
        builder.ConfigureTestServices(services =>
        {
            // Replaces configuration read from Program class on AuctionService.

            // 2.1. DB Context.
            // 2.1.1. Remove current DB Context.
            services.RemoveDbContext<ApplicationDbContext>();
            // 2.1.2. Add test DB Context.
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(this._postgreSqlContainer.GetConnectionString());
            });

            // 2.1.3. Migrate database schema and seed test data.
            services.EnsureCreated<ApplicationDbContext>();

            // 2.2. Service bus (MassTransit Test Harness)
            services.AddMassTransitTestHarness(); // Removes configuration in AuctionService and replaces it with test configuration.

            // 2.3. Set authentication
            // Authenticate against a fake access token that is taken as valid.
            services.AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme)
               .AddFakeJwtBearer(opt =>
               {
                   opt.BearerValueType = FakeJwtBearerBearerValueType.Jwt;
               });
        });
    }
}
