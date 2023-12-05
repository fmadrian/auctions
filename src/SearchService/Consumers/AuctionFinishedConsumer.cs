using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace Consumers;
public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        Console.WriteLine($"Consuming auction {context.Message.Amount} finished");
        // 1. Find the auction using the auction ID sent in the event.
        Item auction = await DB.Find<Item>().OneAsync(context.Message.AuctionId);
        // 2. If the item was sold add information for the winner.
        if (context.Message.ItemSold)
        {
            auction.Winner = context.Message.Winner;
            auction.SoldAmount = (int)context.Message.Amount;
        }
        auction.Status = "Finished";
        await auction.SaveAsync();
    }
}
