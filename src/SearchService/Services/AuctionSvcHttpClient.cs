using System.Web;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Services;
public class AuctionSvcHttpClient
{
    public HttpClient _httpClient { get; set; }
    public IConfiguration _config { get; set; }
    public AuctionSvcHttpClient(HttpClient httpClient, IConfiguration config)
    {
        // Inject dependencies.
        this._httpClient = httpClient;
        this._config = config;
    }

    public async Task<List<Item>> GetItemsForSearchDb()
    {
        // Date of the last updated function.
        Item item = await DB.Find<Item>()
        .Sort(x => x.Descending(x => x.UpdatedAt))
        // .Project(x => x.UpdatedAt.ToString())
        .ExecuteFirstAsync();
        // If the database has never been updated, we use a default datetime.
        string lastUpdated = item == null
            ? DateTimeOffset.MinValue.ToString()
            : item.ToString(); // Fixes formatting issue when converting DateTimeOffset.
        // Encode datetime before passing it as parameter.
        lastUpdated = HttpUtility.UrlEncode(lastUpdated);
        // Queries search endpoint on AuctionService and returns the items.
        // It returns items added from the last time the database was queried.
        string endpointToQuery = $"{_config["AuctionServiceUrl"]}/api/auctions?date={lastUpdated}";
        return await _httpClient.GetFromJsonAsync<List<Item>>(endpointToQuery); // GetFromJsonAsync gets results and serializes.

    }
}
