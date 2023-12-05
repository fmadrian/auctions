using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace Contracts;
public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        // Display the highest bid on the search service.
        Console.WriteLine($"Consuming {context.Message.Id} bid placed");

        Item auction = await DB.Find<Item>().OneAsync(context.Message.AuctionId);
        if (auction.CurrentHighBid > context.Message.Amount && context.Message.BidStatus.Contains("Accepted"))
            auction.CurrentHighBid = context.Message.Amount;

        await auction.SaveAsync();
    }
}
