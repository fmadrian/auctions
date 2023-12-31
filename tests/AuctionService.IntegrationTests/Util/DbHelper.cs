using AuctionService.Entities;

namespace AuctionService.IntegrationTests.Util;
public static class DbHelper
{
    // We use the same database for our all our tests.
    // By default, each test run would create a new database instance inside the web application factory (CustomWebAppFactory)
    // To avoid this (initializing and dropping the database after each test), we are going to use an IClassFixture.
    // Anything inside the web application factory (CustomWebAppFactory) is shared by all the tests.

    // Because we are "reusing" our database for all tests, each test will "contaminate" the test database.
    public static void InitDbForTests(ApplicationDbContext db)
    {
        // Add the auctions we will use to test.
        db.Auctions.AddRange(DbHelper.GetAuctionsForTest());
        db.SaveChanges();
    }
    // After each individual test, we "clean" the database (remove/add data added/modified/deleted by other tests) by reinitializing it.
    public static void ReinitDbForTests(ApplicationDbContext db)
    {
        // Remove all data in the database and readd it.
        db.Auctions.RemoveRange(db.Auctions);
        db.SaveChanges();
        // Add the test auctions.
        DbHelper.InitDbForTests(db);
    }

    public static List<Auction> GetAuctionsForTest()
    {
        return new List<Auction>()
        {
            // 1 Ford GT
            new Auction
            {
                Id = Guid.Parse("afbee524-5972-4075-8800-7d1f9d7b0a0c"),
                Status = Status.Live,
                ReservePrice = 20000,
                Seller = "bob",
                AuctionEnd = DateTime.UtcNow.AddDays(10),
                Item = new Item
                {
                    Make = "Ford",
                    Model = "GT",
                    Color = "White",
                    Mileage = 50000,
                    Year = 2020,
                    ImageUrl = "https://cdn.pixabay.com/photo/2016/05/06/16/32/car-1376190_960_720.jpg"
                }
            },
            // 2 Bugatti Veyron
            new Auction
            {
                Id = Guid.Parse("c8c3ec17-01bf-49db-82aa-1ef80b833a9f"),
                Status = Status.Live,
                ReservePrice = 90000,
                Seller = "alice",
                AuctionEnd = DateTime.UtcNow.AddDays(60),
                Item = new Item
                {
                    Make = "Bugatti",
                    Model = "Veyron",
                    Color = "Black",
                    Mileage = 15035,
                    Year = 2018,
                    ImageUrl = "https://cdn.pixabay.com/photo/2012/05/29/00/43/car-49278_960_720.jpg"
                }
            },
            // 3 Ford mustang
            new Auction
            {
                Id = Guid.Parse("bbab4d5a-8565-48b1-9450-5ac2a5c4a654"),
                Status = Status.Live,
                Seller = "bob",
                AuctionEnd = DateTime.UtcNow.AddDays(4),
                Item = new Item
                {
                    Make = "Ford",
                    Model = "Mustang",
                    Color = "Black",
                    Mileage = 65125,
                    Year = 2023,
                    ImageUrl = "https://cdn.pixabay.com/photo/2012/11/02/13/02/car-63930_960_720.jpg"
                }
            },
        };
    }
}
