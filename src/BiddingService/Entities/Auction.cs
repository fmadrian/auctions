using MongoDB.Entities;

namespace BiddingService.Entities;
public class Auction : Entity
{
    public DateTimeOffset AuctionEnd { get; set; }
    public string Seller { get; set; }
    public decimal ReservePrice { get; set; }
    public bool Finished { get; set; }
}
