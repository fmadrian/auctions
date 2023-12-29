using AuctionService.Controllers;
using AuctionService.Data.Repositories.Interfaces;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelpers;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace AuctionService.UnitTests;

public class AuctionControllerTests
{
    private readonly Mock<IAuctionRepository> _auctionRepo;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly Fixture _fixture;
    private readonly AuctionsController _controller;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<AuctionsController>> _logger;
    public AuctionControllerTests()
    {
        // Everything defined here reinitializes after each test.

        // 1. Instantiate mocks.
        this._fixture = new Fixture();
        this._auctionRepo = new Mock<IAuctionRepository>();
        this._publishEndpoint = new Mock<IPublishEndpoint>();

        this._logger = new Mock<ILogger<AuctionsController>>();

        // 2. Get mapper configuration (already defined on our MappingProfiles).
        var mockMapper = new MapperConfiguration(mc =>
            mc.AddMaps(typeof(MappingProfiles).Assembly)
        ).CreateMapper().ConfigurationProvider;
        this._mapper = new Mapper(mockMapper); // Create a mock of the mapper.

        // 3. Instantiate controller and pass all required dependencies.
        // .Object exposes the mocked object.
        this._controller = new AuctionsController(this._auctionRepo.Object, this._mapper, this._logger.Object, this._publishEndpoint.Object)
        {
            // 4. Instantiate context and use predefined Claims declared on the helpers class.
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = Helpers.GetClaimsPrincipal() }
            }
        };
    }

    /* Test for each endpoint:
    *
    * 1. Returns the expected/right type of response.
    * 2. Bad requests, edge cases.
    * 3. Exceptions (only if they occur in the controller).
    **/

    // Example repository will only have 10 auctions.
    [Fact]
    public async void GetAllAuctions_WithNoParams_ReturnsAllAuctions()
    {
        // 1. Arrange
        // 1.1. Create a list of 10 auctions using Fixture.
        List<AuctionDto> auctions = this._fixture.CreateMany<AuctionDto>(10).ToList();
        // 1.2. Setup repository so that after calling GetAuctionsAsync, it returns the 10 auctions.
        this._auctionRepo.Setup(repo => repo.GetAuctionsAsync(null)).ReturnsAsync(auctions);

        // 2. Act
        // 2.1. Call the mock controller
        var result = await this._controller.GetAllAuctions(null);

        // 3. Assert
        Assert.IsType<ActionResult<List<AuctionDto>>>(result); // Check return type.
        Assert.Equal(10, result.Value.Count); // Check there is 10 auctions in the list.
    }
    [Fact]
    public async void GetAuctionById_WithValidGuid_ReturnsAuction()
    {
        // 1. Arrange
        // 1.1. Create auction object.
        AuctionDto auction = this._fixture.Create<AuctionDto>();
        // 1.2. Setup repository so that after calling GetAuctionById returns the created auction.
        // IMPORTANT: Can be any GUID (It.IsAny<Guid>()) because we are NOT calling/testing the database.        
        this._auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

        // 2. Act
        // IMPORTANT: Can be any GUID (It.IsAny<Guid>()) because we are NOT calling/testing the database.
        var result = await this._controller.GetAuctionById(It.IsAny<Guid>());

        // 3. Assert
        // Check it type of response and then check the auction 'Make' property is correct.
        Assert.IsType<ActionResult<AuctionDto>>(result);
        Assert.Equal(auction.Make, result.Value.Make);
    }
    [Fact]
    public async void GetAuctionById_WithInvalidGuid_Returns404NotFound()
    {
        // 1. Arrange
        this._auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);
        // 2. Act
        var result = await this._controller.GetAuctionById(It.IsAny<Guid>());
        // 3. Assert
        Assert.IsType<NotFoundResult>(result.Result); // 'result' is type NotFoundResult.
    }
    [Fact]
    public async void CreateAuction_WithValidCreateAuctionDto_ReturnsCreatedAtAction()
    {
        // Arrange
        CreateAuctionDto auctionDto = this._fixture.Create<CreateAuctionDto>();
        this._auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        this._auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);
        // Act
        var result = await this._controller.CreateAuction(auctionDto);
        var createdResult = result.Result as CreatedAtActionResult; // Get the Result with the Action Type.
        // Assert
        Assert.NotNull(createdResult);
        Assert.Equal("GetAuctionById", createdResult.ActionName); // Test it returns the correct action name.
        Assert.IsType<AuctionDto>(createdResult.Value); // Test it returns the correct DTO.
    }
    [Fact]
    public async void CreateAuction_FailedSave_Returns400BadRequest()
    {
        // Arrange
        CreateAuctionDto auctionDto = this._fixture.Create<CreateAuctionDto>();
        this._auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        this._auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false); // Set save method to return false.
        // Act
        var result = await this._controller.CreateAuction(auctionDto);
        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async void UpdateAuction_WithValidCreateAuctionDto_ReturnsOk()
    {
        // 1. Arrange

        // 1.1. IMPORTANT: We have to create an EXISTENT auction and its item separately.
        // while excluding the related item from each one of them.
        Auction auction = this._fixture.Build<Auction>().Without(a => a.Item).Create(); // Auction without item.
        // 1.1.1. Create an item and add it as the auction's item (it will be modified by the controller).
        Item item = this._fixture.Build<Item>().Without(i => i.Auction).Create();
        auction.Item = item;
        // 1.1.2. Manually put 'Seller'
        auction.Seller = "testUsername"; // Has to match 'Name' claim passed in the constructor.

        // 1.3. Create DTO.
        UpdateAuctionDto updatedDto = this._fixture.Create<UpdateAuctionDto>();

        // 1.4. Setup repository methods to return expected values.
        this._auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        this._auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        // 2. Act
        var result = await this._controller.UpdateAuction(auction.Id, updatedDto);

        // 3. Assert
        Assert.IsType<OkResult>(result);
    }
    [Fact]
    public async void UpdateAuction_WithInvalidGuid_Returns404NotFound()
    {
        // 1. Arrange

        // 1.1. IMPORTANT: We have to create an EXISTENT auction and its item separately.
        // while excluding the related item from each one of them.
        Auction auction = this._fixture.Build<Auction>().Without(a => a.Item).Create();

        // 1.2. Create DTO.
        UpdateAuctionDto updatedDto = this._fixture.Create<UpdateAuctionDto>();

        // 1.3. Setup repository.
        this._auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(value: null);

        // 2. Act
        var result = await this._controller.UpdateAuction(auction.Id, updatedDto);

        // 3. Assert
        Assert.IsType<NotFoundResult>(result); // 'result' is type NotFoundResult.

    }
    [Fact]
    public async void UpdateAuction_WithDifferentSellerName_Returns403Forbid()
    {
        // 1. Arrange

        // 1.1. IMPORTANT: We have to create an EXISTENT auction and its item separately.
        // while excluding the related item from each one of them.
        Auction auction = this._fixture.Build<Auction>().Without(a => a.Item).Create(); // Auction without item.
        auction.Seller = "differentSeller"; // Different seller from test user we are using to call endpoint.
        // 1.2. Create DTO.
        UpdateAuctionDto updatedDto = this._fixture.Create<UpdateAuctionDto>();

        // 1.3. Setup repository.
        this._auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);

        // 2. Act
        var result = await this._controller.UpdateAuction(auction.Id, updatedDto);

        // 3. Assert
        Assert.IsType<ForbidResult>(result); // 'result' is ForbidResult because seller is different from the 'current' user.
    }

    [Fact]
    public async void UpdateAuction_FailedSave_Returns400BadRequest()
    {
        // 1. Arrange
        // 1.1. IMPORTANT: We have to create an EXISTENT auction and its item separately.
        // while excluding the related item from each one of them.
        Auction auction = this._fixture.Build<Auction>().Without(a => a.Item).Create(); // Auction without item.
        // 1.1.1. Create an item and add it as the auction's item (it will be modified by the controller).
        Item item = this._fixture.Build<Item>().Without(i => i.Auction).Create();
        auction.Item = item;
        // 1.1.2. Manually put 'Seller'
        auction.Seller = "testUsername"; // Has to match 'Name' claim passed in the constructor.

        // 1.3. Create DTO.
        UpdateAuctionDto updatedDto = this._fixture.Create<UpdateAuctionDto>();

        // 1.4. Setup repository methods to return expected values.
        this._auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        this._auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false); // Test will fail save.

        // 2. Act
        var result = await this._controller.UpdateAuction(auction.Id, updatedDto);

        // 3. Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
    [Fact]
    public async void DeleteAuction_WithValidUser_ReturnsOk()
    {
        // 1. Arrange
        // 1.1. Objects.
        Auction auction = this._fixture.Build<Auction>()
                                .With(a => a.Seller, "testUsername")
                                .Without(a => a.Item).Create(); // With is another way to set the value of an attribute in the created object.
        // 1.2. Repository methods and their expected results.
        this._auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        this._auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        // 2. Act
        var result = await this._controller.DeleteAuction(auction.Id);

        // 3. Assert
        Assert.NotNull(result);
        Assert.IsType<OkResult>(result);
    }
    [Fact]
    public async void DeleteAuction_WithInvalidGuid_Returns404NotFound()
    {
        // 1. Arrange
        // 1.1. Objects
        Auction auction = this._fixture.Build<Auction>().Without(a => a.Item).Create();
        // 1.2. Repository.
        this._auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(value: null);
        this._auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        // 2. Act
        var result = await this._controller.DeleteAuction(It.IsAny<Guid>());

        // 3. Assert
        Assert.IsType<NotFoundResult>(result);

    }
    [Fact]
    public async void DeleteAuction_WithDifferentSellerName_Returns403Forbid()
    {
        // 1. Arrange
        // 1.1. Objects
        Auction auction = this._fixture.Build<Auction>().Without(a => a.Item).Create();
        // 1.2. Repositories
        this._auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        // 2. Act
        ActionResult result = await this._controller.DeleteAuction(It.IsAny<Guid>());
        // 3. Assert
        Assert.IsType<ForbidResult>(result);
    }
    [Fact]
    public async void DeleteAuction_FailedSave_Returns400BadRequest()
    {
        // Arrange
        // 1.1. Objects
        Auction auction = this._fixture.Build<Auction>()
                                .With(a => a.Seller, "testUsername")
                                .Without(a => a.Item).Create();
        // 1.2. Repositories
        this._auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        this._auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

        // 2. Act
        ActionResult result = await this._controller.DeleteAuction(It.IsAny<Guid>());
        // 3. Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}