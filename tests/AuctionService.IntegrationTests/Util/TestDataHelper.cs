using AuctionService.DTOs;

namespace AuctionService.IntegrationTests.Util;
public static class TestDataHelper
{
    #region Internal auction creation methods 

    public static CreateAuctionDto GetAuctionForCreate(bool isValid = true)
    {
        return new CreateAuctionDto()
        {
            Make = isValid ? "testMake" : null,
            Model = "testModel",
            ImageUrl = "testImageUrl",
            Mileage = 10,
            Year = 20,
            ReservePrice = 30,
            Color = "testColor",
            AuctionEnd = DateTimeOffset.UtcNow.AddDays(1)
        };
    }
    public static UpdateAuctionDto GetAuctionForUpdate(bool isValid = true)
    {
        if (!isValid)
        {
            return new UpdateAuctionDto() { };
        }
        return new UpdateAuctionDto()
        {
            Make = "testMake",
            Model = "testModel",
            Mileage = 10,
            Year = 20,
            Color = "testColor",
        };
    }
    #endregion
}
