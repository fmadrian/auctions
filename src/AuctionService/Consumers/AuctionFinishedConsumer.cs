using AuctionService.Entities;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Consumers;
public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
    // Inject the database context.
    private readonly ApplicationDbContext _dbContext;

    public AuctionFinishedConsumer(ApplicationDbContext dbContext)
    {
        this._dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        Console.WriteLine($"Consuming auction {context.Message.Amount} finished");
        // 1. Find the auction using the auction ID sent in the event.
        Auction auction = await this._dbContext.Auctions.FindAsync(Guid.Parse(context.Message.AuctionId));
        // 2. If the item was sold add information for the winner.
        if (context.Message.ItemSold)
        {
            auction.Winner = context.Message.Winner;
            auction.SoldAmount = context.Message.Amount;
        }
        // 3. Set the status according to if the reserve price was met by the final amount.
        auction.Status = auction.SoldAmount >= auction.ReservePrice ? Status.Finished : Status.ReserveNotMet;
        // 4. Save changes in database.
        await this._dbContext.SaveChangesAsync();
    }
}
