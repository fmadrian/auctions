using MassTransit;
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
        // Outbox configuration.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Recommend to call the parent method.
            // Outbox configuration.
            // Adds three tables to the database.
            // Responsible for the outbox functionality.
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

        }
    }
}