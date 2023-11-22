using MongoDB.Entities;

namespace SearchService.Entities
{
    // Has to inherit from MongoDB entity 
    // Because it provides ID for each document on this collection.
    public class Item : Entity
    {
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset AuctionEnd { get; set; }
        public string Seller { get; set; }
        public string Winner { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }
        public int Mileage { get; set; }
        public string ImageUrl { get; set; }
        public string Status { get; set; }
        public decimal ReservePrice { get; set; }
        public decimal? SoldAmount { get; set; }
        public decimal? CurrentHighBid { get; set; }
    }
}