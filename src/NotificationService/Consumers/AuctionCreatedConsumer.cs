using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;
public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    // 1. Inject Hub context.
    private readonly IHubContext<NotificationHub> _hubContext;
    public AuctionCreatedConsumer(IHubContext<NotificationHub> hubContext)
    {
        // 1. Inject Hub context.
        this._hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine("==> Sending notification to clients : AUCTION CREATED");

        // 2. Send a message to all the connected clients.
        // AuctionCreated is the name of the method client side of SignalR listens for.
        // context.Message is the object the clients receive when the method is used.
        await _hubContext.Clients.All.SendAsync("AuctionCreated", context.Message);
    }
}
