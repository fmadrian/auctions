using AuctionService.Entities;
using AuctionService;
using Grpc.Core;

namespace AuctionService.Services;
public class GrpcAuctionService : GrpcAuction.GrpcAuctionBase
{
    private readonly ApplicationDbContext _dbContext;
    public GrpcAuctionService(ApplicationDbContext dbContext)
    {
        this._dbContext = dbContext;
    }
    public override async Task<GrpcAuctionResponse> GetAuction(GetAuctionRequest request, ServerCallContext context)
    {
        Console.WriteLine("Received Grpc request for auction");
        // Search auction.
        Auction auction = await _dbContext.Auctions.FindAsync(Guid.Parse(request.Id));
        if (auction == null) throw new RpcException(new Grpc.Core.Status(StatusCode.NotFound, "Not found"));
        // Send message to GRPC server.
        GrpcAuctionResponse response = new GrpcAuctionResponse()
        {
            Auction = new GrpcAuctionModel()
            {
                Id = auction.Id.ToString(),
                Seller = auction.Seller,
                AuctionEnd = auction.AuctionEnd.ToString(),
                ReservePrice = (DecimalValue)auction.ReservePrice
            }
        };
        return response;
    }
}
