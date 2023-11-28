using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Consumers;
public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
{
    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
        Console.WriteLine($"Deleting update auction {context.Message.Id}.");

        // 1. Retrieve ID from message.
        string id = context.Message.Id;
        // 2. Delete object from database.
        var result = await DB.DeleteAsync<Item>(id);
        // 3. Check whether the operation was completed.
        if (!result.IsAcknowledged)
            throw new MessageException(typeof(AuctionDeleted), "Couldn't delete document from MongoDB.");

    }
}
