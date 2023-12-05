namespace AuctionService.Entities
{

    public class Auction
    {
        public Guid Id { get; set; }
        public decimal ReservePrice { get; set; } = 0;

        public string Seller { get; set; } // Username from the seller.
        public string Winner { get; set; } // Username from the winner.
        public decimal? SoldAmount { get; set; }
        public decimal? CurrentHighBid { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset AuctionEnd { get; set; }
        public Status Status { get; set; } = Status.Live;
        public Item Item { get; set; }
    }
}