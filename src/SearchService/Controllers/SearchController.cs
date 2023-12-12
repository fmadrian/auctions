using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Entities;
using SearchService.RequestHelpers;

namespace SearchService.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
        // [FromQuery] tells API to search string parameters in the request and match them into a SearchParams object.
        {
            // 1. Define query
            // 2. Execute query and return result.

            // Find in the Items collection (second item is for the filters).
            var query = DB.PagedSearch<Item, Item>();
            // Sort them by the Make property.
            // query.Sort(x => x.Ascending(item => item.Make)); (Not necessary, already defined in filters).

            if (!string.IsNullOrEmpty(searchParams.SearchTerm))
            {
                // Define filters for the query.
                query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
            }
            // Specify order by parameters.
            query = searchParams.OrderBy switch
            {
                "make" => query.Sort(x => x.Ascending(a => a.Make))
                                .Sort(x => x.Ascending(a => a.Model)), // Sort results by make and then model.
                "new" => query.Sort(x => x.Ascending(a => a.CreatedAt)),
                _ => query.Sort(x => x.Ascending(a => a.AuctionEnd)) // Default case
            };
            // Specify filter by parameters
            query = searchParams.FilterBy switch
            {
                "finished" => query.Match(x => x.AuctionEnd < DateTimeOffset.UtcNow),
                // In between now and 6 hours
                "endingSoon" => query.Match(x => x.AuctionEnd < DateTimeOffset.UtcNow.AddHours(6)
                                                && x.AuctionEnd > DateTimeOffset.UtcNow),
                _ => query.Match(x => x.AuctionEnd > DateTimeOffset.UtcNow) // Default case (live auctions)
            };
            // Filter by seller (if we pass the filter).
            if (!string.IsNullOrEmpty(searchParams.Seller))
                query.Match(x => x.Seller == searchParams.Seller);
            // Filter by winner (if we pass the filter).
            if (!string.IsNullOrEmpty(searchParams.Winner))
                query.Match(x => x.Winner == searchParams.Winner);


            // Define pagination parameters.
            query.PageNumber(searchParams.PageNumber);
            query.PageSize(searchParams.PageSize);
            // Execute the query and return the result.
            var result = await query.ExecuteAsync();
            return Ok(new
            {
                results = result.Results,
                pageCount = result.PageCount,
                totalCount = result.TotalCount
            });
        }
    }
}
