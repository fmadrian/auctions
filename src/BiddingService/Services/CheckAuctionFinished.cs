
using BiddingService.Entities;
using BiddingService.Entities.Enums;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService.Services;
public class CheckAuctionFinished : BackgroundService
{
    private readonly ILogger<CheckAuctionFinished> _logger;
    private readonly IServiceProvider _provider;
    public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider provider)
    {
        this._logger = logger;
        this._provider = provider;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this._logger.LogInformation("===> Checking for finished auctions (background service).");
        // 1. Register what to do when the stopping token is called.
        stoppingToken.Register(() =>
            this._logger.LogWarning("===> Stopped checking for finished auctions (background service).")
        );
        // 2. Indicate what to do while the service is on (stopping token not triggered).
        while (!stoppingToken.IsCancellationRequested)
        {
            // 2.1. What to do?
            await CheckAuctions(stoppingToken);
            // 2.2. How often?
            await Task.Delay(5000, stoppingToken);
        }
        // REMEMBER: Add service in the Program class.
    }
    private async Task CheckAuctions(CancellationToken stoppingToken)
    {
        // 1. Find all auctions that...
        List<Auction> finishedAuctions = await DB.Find<Auction>()
        .Match(a => a.AuctionEnd <= DateTimeOffset.UtcNow) // Have gone past the auction end time.
        .Match(a => !a.Finished) // And have not being tagged as Finished.
        .ExecuteAsync(stoppingToken); // Always pass the stopping token.
        // 2. Check if there are finished auctions. (If there aren't, get out of the function.)
        if (finishedAuctions.Count == 0) return;

        // 3. Log the action.
        this._logger.LogInformation("===> Marked {count} auctions marked as finished.", finishedAuctions.Count);

        // 4. IMPORTANT: Create scope to access service provider.
        using var scope = this._provider.CreateScope();
        // 5. IMPORTANT: Get publish endpoint using scope created.
        var publishEndpoint = scope.ServiceProvider.GetService<IPublishEndpoint>();

        // 6. Make changes and send/publish them in the service bus.
        foreach (Auction auction in finishedAuctions)
        {
            // Mark auctions as finished.
            auction.Finished = true;
            await auction.SaveAsync(null, stoppingToken);
            // Mark the winning bid (Highest that met reserve price).
            Bid winningBid = await DB.Find<Bid>()
                                    .Match(b => b.AuctionId == auction.ID)
                                    .Match(b => b.BidStatus == BidStatus.Accepted)
                                    .Sort(x => x.Descending(b => b.Amount))
                                    .ExecuteFirstAsync(stoppingToken);

            // Publish the event.
            await publishEndpoint.Publish(new AuctionFinished()
            {
                AuctionId = auction.ID,
                ItemSold = winningBid != null,
                Amount = winningBid?.Amount,
                Winner = winningBid?.Bidder,
                Seller = auction.Seller
            }, stoppingToken);
        }

    }
}
