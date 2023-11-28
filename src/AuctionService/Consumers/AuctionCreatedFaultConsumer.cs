using Contracts;
using MassTransit;

namespace AuctionService.Consumers;
public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreated>>
{
    public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
    {
        Console.WriteLine("Consuming faulty AuctionCreated.");
        var exception = context.Message.Exceptions.First();
        // System.ArgumentException is one of the many exceptions that could throw/cause a faulty event.
        if (exception.ExceptionType == "System.ArgumentException")
        {
            // Exception expected is Model named 'Foo'.
            // context.Message.Message has the object.
            // Rename model to "Renamed model"
            context.Message.Message.Model = "Renamed model";
            // Send a this new Event to the Service Bus.
            await context.Publish(context.Message.Message);
        }
        else
        {
            Console.WriteLine("Unknown error.");
        }
    }
}
