using BiddingService.Entities.Enums;
using MongoDB.Entities;

namespace BiddingService.Entities;
public class Bid : Entity
{
    public string AuctionId { get; set; }
    public DateTimeOffset BidTime { get; set; } = DateTimeOffset.UtcNow;
    public string Bidder { get; set; }
    public decimal Amount { get; set; }
    public BidStatus BidStatus { get; set; }

}
