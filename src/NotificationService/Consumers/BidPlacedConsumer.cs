using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;
public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    // 1. Inject Hub context.
    private readonly IHubContext<NotificationHub> _hubContext;
    public BidPlacedConsumer(IHubContext<NotificationHub> hubContext)
    {
        // 1. Inject Hub context.
        this._hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("==> Sending notification to clients : BID PLACED");

        // 2. Send a message to all the connected clients.
        // BidPlaced is the name of the method client side of SignalR listens for.
        // context.Message is the object the clients receive when the method is used.
        await _hubContext.Clients.All.SendAsync("BidPlaced", context.Message);
    }
}
