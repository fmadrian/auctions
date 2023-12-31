using System.Net;
using System.Net.Http.Json;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Contracts;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

// Every test classes uses an instance of IClassFixture
// This means two databases, two servers, and so on.
[Collection("SharedCollection")]
public class AuctionBusTests : IAsyncLifetime
{
    // 1. Inject web factory and create http client.
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    // 3. Setup service bus tests.
    private readonly ITestHarness _testHarness;
    public AuctionBusTests(CustomWebAppFactory factory)
    {
        // 1. Inject web factory and create http client.
        this._factory = factory;
        this._httpClient = factory.CreateClient();
        // 3. Setup service bus tests.
        this._testHarness = factory.Services.GetTestHarness();
    }

    [Fact]
    public async Task CreateAuction_WithValidObject_PublishesAuctionCreated()
    {
        // 1. Arrange
        string username = "testUsername";
        CreateAuctionDto dto = TestDataHelper.GetAuctionForCreate();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(username));
        // 2. Act
        HttpResponseMessage response = await this._httpClient.PostAsJsonAsync("api/auctions", dto);
        // 3. Assert
        response.EnsureSuccessStatusCode(); // Ensure a successful response.
        Assert.True(await this._testHarness.Published.Any<AuctionCreated>()); // Checks the harness to see if an AuctionCreated message was published.
    }

    #region Initialization and disposal methods
    public Task InitializeAsync()
    {
        // Because this is NOT a FIXTURE, this initializes before every test.
        return Task.CompletedTask;
    }
    public Task DisposeAsync()
    {
        // Because this is NOT a FIXTURE, this runs after every test.

        // 2. After every test, we reinitialize the database.

        // 2.1. Create scope to access the application database context.
        var scope = this._factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // 2.2. Call reinitialize method.
        DbHelper.ReinitDbForTests(dbContext);
        return Task.CompletedTask;
    }
    #endregion
}


