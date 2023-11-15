using Microsoft.EntityFrameworkCore;

namespace AuctionService.Entities
{
    public class ApplicationDbContext : DbContext
    {
        // Entities (DB sets)
        public DbSet<Auction> Auctions { get; set; }
        // public DbSet<Item> Items { get; set; } // We don't need to reference it, because its already related by Auctions.
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }
    }
}