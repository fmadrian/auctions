using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Entities;
using SearchService.Services;

namespace SearchService.Data
{
    public class DBInitializer
    {
        public async static Task InitDb(WebApplication app)
        {
            // Initiate MongoDB database.
            await DB.InitAsync("SearchDb", MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

            // Add index for the fields we want to search from.
            await DB.Index<Item>()
                .Key(item => item.Make, KeyType.Text)
                .Key(item => item.Model, KeyType.Text)
                .Key(item => item.Color, KeyType.Text)
                .CreateAsync();

            // Counts all documents in Item collection
            long count = await DB.CountAsync<Item>();
            // Create scope to access service before it is properly injected.
            using var scope = app.Services.CreateScope();
            AuctionSvcHttpClient httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();
            List<Item> items = await httpClient.GetItemsForSearchDb();
            Console.WriteLine($"{items.Count} returned from the auction service.");
            // Add items if there are any.
            if (items.Count > 0)
                await DB.SaveAsync(items);
        }

    }
}