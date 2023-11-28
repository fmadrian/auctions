using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Consumers
{
    public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
    {
        private readonly IMapper _mapper;
        public AuctionCreatedConsumer(IMapper mapper)
        {
            this._mapper = mapper;
        }
        public async Task Consume(ConsumeContext<AuctionCreated> context)
        {
            Console.WriteLine($"Consuming action created. {context.Message.Id}");
            // Retrieve the object/contract (contract.Message) and map it into an Item entity.
            Item item = this._mapper.Map<Item>(context.Message);

            if (item.Model == "Foo") throw new ArgumentException("Model name in Item cannot be 'Foo'.");

            // Save the item into the database.
            await DB.InsertAsync(item);

            // Doesn't return anything.
        }
    }
}