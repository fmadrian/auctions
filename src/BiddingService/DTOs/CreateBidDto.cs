namespace BiddingService.DTOs;
public class BidDto
{
    public string Id { get; set; }
    public string AuctionId { get; set; }
    public DateTimeOffset BidTime { get; set; }
    public string Bidder { get; set; }
    public decimal Amount { get; set; }
    public string BidStatus { get; set; }

}
