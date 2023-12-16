
using BiddingService.Entities;
using Grpc.Net.Client;

namespace BiddingService.Services;
public class GrpcAuctionClient
{
    private readonly ILogger<GrpcAuctionClient> _logger;
    private readonly IConfiguration _config;
    public GrpcAuctionClient(ILogger<GrpcAuctionClient> logger, IConfiguration config)
    {
        this._logger = logger;
        this._config = config;
    }

    public Auction GetAuction(string id)
    {
        this._logger.LogInformation("Calling gRPC service");
        var channel = GrpcChannel.ForAddress(this._config["GrpcAuction"]);

        var client = new GrpcAuction.GrpcAuctionClient(channel);
        // Receive request from gRPC server and parse object.
        var request = new GetAuctionRequest() { Id = id };
        try
        {
            var reply = client.GetAuction(request);
            var auction = new Auction()
            {
                ID = reply.Auction.Id,
                Seller = reply.Auction.Seller,
                AuctionEnd = DateTimeOffset.Parse(reply.Auction.AuctionEnd),
                ReservePrice = reply.Auction.ReservePrice
            };
            return auction;
        }
        catch (Exception e)
        {
            this._logger.LogError(e, "Could not process data from gRPC server");
            throw;
        }
    }

}
