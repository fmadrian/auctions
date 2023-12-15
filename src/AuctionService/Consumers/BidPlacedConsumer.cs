using AuctionService.Entities;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;
public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    private readonly ApplicationDbContext _dbContext;
    public BidPlacedConsumer(ApplicationDbContext dbContext)
    {
        this._dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine($"Consuming bid {context.Message.Id} placed");
        // 1. Search the auction and change the bid information
        Auction auction = await this._dbContext.Auctions.FindAsync(Guid.Parse(context.Message.AuctionId));
        int bidPlaced = context.Message.Amount;
        if (auction.CurrentHighBid == null ||
        (context.Message.BidStatus.Contains("Accepted") && bidPlaced > auction.CurrentHighBid))
        {
            auction.CurrentHighBid = bidPlaced;
            await this._dbContext.SaveChangesAsync();
        }
    }
}
