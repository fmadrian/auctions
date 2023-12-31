using System.Net;
using System.Net.Http.Json;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;
// By inheriting from IClassFixture<CustomWebAppFactory>, we share 
// the same instance of CustomWebAppFactory (and therefore the test database) across all the tests.
[Collection("SharedCollection")]
public class AuctionControllerTests : IAsyncLifetime
{
    // 1. Inject web factory and create http client.
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    public AuctionControllerTests(CustomWebAppFactory factory)
    {
        this._factory = factory;
        this._httpClient = factory.CreateClient();
    }

    #region GetAuctions
    [Fact]
    public async Task GetAuctions_ReturnsAllAuctions()
    {
        // This tests the endpoint using the test database.

        // Nothing to arrange.

        // 1. Act
        // Make a call to the controller.
        var response = await this._httpClient.GetFromJsonAsync<List<AuctionDto>>("api/auctions");

        // 2. Assert
        Assert.Equal(3, response.Count); // We only have 3 auctions in the database.
    }

    [Fact]
    public async Task GetAuctionById_WithValidId_ReturnsAuction()
    {
        // 1. Arrange.
        Auction auction = DbHelper.GetAuctionsForTest()[0];
        Guid guid = auction.Id;
        // 2. Act
        // Make a call to the controller.
        var response = await this._httpClient.GetFromJsonAsync<AuctionDto>($"api/auctions/{guid}");

        // 3. Assert
        // We compare the response to the auction at the beginning of test.
        Assert.Equal(auction.Item.Model, response.Model);
    }
    [Fact]
    public async Task GetAuctionById_WithInvalidId_Returns404NotFound()
    {
        // 1. Arrange
        Guid guid = Guid.NewGuid();
        // 2. Act
        // GetAsync let us access the response code.
        var response = await this._httpClient.GetAsync($"api/auctions/{guid}");

        // 3. Assert
        // Status code returned should be 404.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    [Fact]
    public async Task GetAuctionById_WithInvalidId_Returns400BadRequest()
    {
        // 1. Arrange
        string guid = "invalidguid";
        // 2. Act
        // GetAsync let us access the response code.
        var response = await this._httpClient.GetAsync($"api/auctions/{guid}");

        // 3. Assert
        // Status code returned should be 404.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    #endregion

    #region CreateAuction
    [Fact]
    public async Task CreateAuction_WithNoAuth_Returns401Unauthorized()
    {
        // 1. Arrange
        CreateAuctionDto dto = TestDataHelper.GetAuctionForCreate();
        // 2. Act
        var response = await this._httpClient.PostAsJsonAsync($"api/auctions", dto);
        // 3. Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    [Fact]
    public async Task CreateAuction_WithAuth_Returns201Created()
    {

        // 1. Arrange
        string username = "testUsername";
        CreateAuctionDto dto = TestDataHelper.GetAuctionForCreate();
        // 2. Act
        // 2.1. Pass the fake bearer token in the request.
        this._httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(username));
        var response = await this._httpClient.PostAsJsonAsync($"api/auctions", dto);
        // 3. Assert
        response.EnsureSuccessStatusCode(); // Getting NOT a failure.
        Assert.Equal(HttpStatusCode.Created, response.StatusCode); // Test the response code.

        // 3.1. Check the value of seller (user) is equal to the user that is passing the token.
        AuctionDto createdAuction = await response.Content.ReadFromJsonAsync<AuctionDto>();
        Assert.Equal(createdAuction.Seller, username);
    }
    [Fact]
    public async Task CreateAuction_WithInvalidDto_Returns400BadRequest()
    {

        // 1. Arrange
        string username = "testUsername";
        CreateAuctionDto dto = TestDataHelper.GetAuctionForCreate(false); // Returns invalid DTO.
        // 2. Act
        // 2.1. Pass the fake bearer token in the request.
        this._httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(username));
        var response = await this._httpClient.PostAsJsonAsync($"api/auctions", dto);
        // 3. Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); // Test the response code.
    }
    #endregion

    #region UpdateAuction
    [Fact]
    public async Task UpdateAuction_WithValidDto_ReturnsCreated()
    {
        // 1. Arrange
        Auction auctionToBeUpdated = DbHelper.GetAuctionsForTest()[0];
        string username = auctionToBeUpdated.Seller; // Should be 'bob'.
        string id = auctionToBeUpdated.Id.ToString();
        this._httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(username));
        UpdateAuctionDto dto = TestDataHelper.GetAuctionForUpdate();
        // 2. Act
        HttpResponseMessage response = await this._httpClient.PutAsJsonAsync($"api/auctions/{id}", dto);
        // 3. Assert status code.
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    [Fact]
    public async Task UpdateAuction_WithInvalidUser_Returns403Forbidden()
    {
        // 1. Arrange
        Auction auctionToBeUpdated = DbHelper.GetAuctionsForTest()[0];
        string username = "aDifferentUser", id = auctionToBeUpdated.Id.ToString();
        this._httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(username));
        UpdateAuctionDto dto = TestDataHelper.GetAuctionForUpdate();
        // 2. Act
        HttpResponseMessage response = await this._httpClient.PutAsJsonAsync($"api/auctions/{id}", dto);
        // 3. Assert status code.
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    [Fact]
    public async Task UpdateAuction_WithInvalidDto_Returns400BadRequest()
    {
        // 1. Arrange
        Auction auctionToBeUpdated = DbHelper.GetAuctionsForTest()[0];
        string username = auctionToBeUpdated.Seller, id = auctionToBeUpdated.Id.ToString();
        this._httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(username));
        UpdateAuctionDto dto = TestDataHelper.GetAuctionForUpdate(false);
        // 2. Act
        HttpResponseMessage response = await this._httpClient.PutAsJsonAsync($"api/auctions/{id}", dto);
        // 3. Assert status code.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    [Fact]
    public async Task UpdateAuction_WithInvalidId_Returns404NotFound()
    {
        string username = "testUsername";
        this._httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(username));
        // Pass a made up GUID to ensure the test returns the expected result.
        string id = Guid.NewGuid().ToString();
        UpdateAuctionDto dto = TestDataHelper.GetAuctionForUpdate(true);
        // 2. Act
        HttpResponseMessage response = await this._httpClient.PutAsJsonAsync($"api/auctions/{id}", dto);
        // 3. Assert status code.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    [Fact]
    public async Task UpdateAuction_WithNoUser_Returns401Unauthorized()
    {
        // We are attempting to access the endpoint without a JWT.
        string id = DbHelper.GetAuctionsForTest()[0].Id.ToString(); // Could be any GUID, not relevant for this test.
        UpdateAuctionDto dto = TestDataHelper.GetAuctionForUpdate(true);
        // 2. Act
        HttpResponseMessage response = await this._httpClient.PutAsJsonAsync($"api/auctions/{id}", dto);
        // 3. Assert status code.
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    #endregion

    #region DeleteAuction
    [Fact]
    public async Task DeleteAuction_WithValidIdAndUser_Returns200Ok()
    {
        Auction auction = DbHelper.GetAuctionsForTest()[0];
        string username = auction.Seller, id = auction.Id.ToString();
        this._httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(username));

        HttpResponseMessage response = await this._httpClient.DeleteAsync($"api/auctions/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    [Fact]
    public async Task DeleteAuction_WithInvalidId_Returns400BadRequest()
    {
        Auction auction = DbHelper.GetAuctionsForTest()[0];
        string username = auction.Seller, id = "invalidGuid";
        this._httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(username));

        HttpResponseMessage response = await this._httpClient.DeleteAsync($"api/auctions/{id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    [Fact]
    public async Task DeleteAuction_WithInvalidId_Returns404NotFound()
    {
        // 1.1. Get auction and information we need from it.
        Auction auction = DbHelper.GetAuctionsForTest()[0];
        string username = auction.Seller, id = Guid.NewGuid().ToString();
        // 1.2. Pass fake token.
        this._httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(username));
        // 2. Act
        HttpResponseMessage response = await this._httpClient.DeleteAsync($"api/auctions/{id}");
        // 3. Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    [Fact]
    public async Task DeleteAuction_WithDifferentUserAndSeller_Returns403Forbidden()
    {
        // 1.1. Get auction.
        Auction auction = DbHelper.GetAuctionsForTest()[0];
        // 1.2. Pass fake token.
        string username = "differentUser", id = auction.Id.ToString();
        this._httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(username));
        // 2. Act
        HttpResponseMessage response = await this._httpClient.DeleteAsync($"api/auctions/{id}");
        // 3. Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    [Fact]
    public async Task DeleteAuction_WithNoUser_Returns401Unauthorized()
    {
        Auction auction = DbHelper.GetAuctionsForTest()[0];
        string id = auction.Id.ToString();
        HttpResponseMessage response = await this._httpClient.DeleteAsync($"api/auctions/{id}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    #endregion

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
