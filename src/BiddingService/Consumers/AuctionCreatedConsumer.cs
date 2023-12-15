using BiddingService.Entities;
using Contracts;
using MassTransit;
using MongoDB.Entities;


namespace BiddingService.Consumers;
public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        // Capture object received from event and store it into this service's database.
        var message = context.Message;
        Auction auction = new Auction()
        {
            ID = message.Id.ToString(),
            Seller = message.Seller,
            AuctionEnd = message.AuctionEnd,
            ReservePrice = message.ReservePrice
        };

        await DB.SaveAsync(auction);

    }
}
