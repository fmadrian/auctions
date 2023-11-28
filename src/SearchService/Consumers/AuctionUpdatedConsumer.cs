namespace SearchService.Consumers;

using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
    private readonly IMapper _mapper;
    public AuctionUpdatedConsumer(IMapper mapper)
    {
        this._mapper = mapper;
    }
    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        Console.WriteLine($"Consuming update auction {context.Message.Id}.");
        // 1. Retrieve message object.
        AuctionUpdated message = context.Message;
        // 2. Retrieve the Auction ID
        string id = context.Message.Id;
        // 3. Update auction in this database.
        var result = await DB.Update<Item>()
        .MatchID(message.Id) // SearchDB.Items.Id == AuctionDB.Auction.Id
        .ModifyOnly(item => new { item.Make, item.Model, item.Color, item.Mileage, item.Year }, this._mapper.Map<Item>(message))
        .ExecuteAsync();

        if (!result.IsAcknowledged)
            throw new MessageException(typeof(AuctionUpdated), "Problem updating MongoDB.");
    }
}
