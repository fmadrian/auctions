using AutoMapper;
using BiddingService.DTOs;
using BiddingService.Entities;
using BiddingService.Entities.Enums;
using BiddingService.Services;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;

namespace BiddingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BidsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly GrpcAuctionClient _grpcClient;

    public BidsController(IMapper mapper, IPublishEndpoint publishEndpoint, GrpcAuctionClient grpcClient)
    {
        this._mapper = mapper;
        this._publishEndpoint = publishEndpoint;
        this._grpcClient = grpcClient;

    }
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<BidDto>> PlaceBid(string auctionId, int amount)
    {
        // When an auction is created, the auction service will send an event
        // that will be consumed by this service. This will result in the auction
        // passed by the AuctionService being added to the database in this service.

        // 1. Check on this service's database to see if the auction exists.
        Auction auction = await DB.Find<Auction>().OneAsync(auctionId);

        if (auction == null)
        {
            // 2. If it doesn't exist on this service's database, try to find it in the auction service.
            // TODO: Check with auction service if the auction exists.
            return NotFound();
        }

        // 3. Check the seller is not the one who makes the bid.
        // User.Identity.Name comes from the claims in the JWT.
        if (auction.Seller == User.Identity.Name)
        {
            return BadRequest("You can't bid on your own auction.");
        }

        // 4. Check we can put a bid on this auction.
        Bid bid = new Bid()
        {
            Amount = amount,
            AuctionId = auctionId,
            Bidder = User.Identity.Name
        };
        // Register the bid as finished if the auction ended.
        if (auction.AuctionEnd < DateTimeOffset.UtcNow)
        {
            // Don't register the bid, but put it in finished status.
            bid.BidStatus = BidStatus.Finished;
        }
        else
        {

            // Search the highest bid for the auction to check the status of the auction. 
            var highestBid = await DB.Find<Bid>()
                .Match(a => a.ID == auctionId) // Search all bids for this auction.
                .Sort(a => a.Descending(x => x.Amount)) // Sort them by Amount descending.
                .ExecuteFirstAsync(); // Get the first result.

            // Check if there are no bids or this is the highest.
            if (highestBid != null && amount > highestBid.Amount || highestBid == null)
            {
                // Check if whether it meets the reserve price
                bid.BidStatus = amount >= auction.ReservePrice ?
                BidStatus.Accepted : BidStatus.AcceptedBelowReserve;
            }

            // Check if the bid is lower than the highest bid.
            if (highestBid != null && amount < highestBid.Amount)
            {
                bid.BidStatus = BidStatus.TooLow;
            }
        }
        // Store in database.
        await DB.SaveAsync(bid);
        // Map bid into a BidPlaced event and send/publish it to the service bus.
        await this._publishEndpoint.Publish(this._mapper.Map<BidPlaced>(bid));
        // Return result.
        return Ok(this._mapper.Map<BidDto>(bid));

    }
    [HttpGet("{auctionId}")]
    public async Task<ActionResult<List<BidDto>>> GetBidsForAuction(string auctionId)
    {
        List<Bid> bids = await DB.Find<Bid>()
                                .Match(b => b.AuctionId == auctionId)
                                .Sort(s => s.Descending(x => x.BidTime))
                                .ExecuteAsync();

        return bids.Select(this._mapper.Map<BidDto>).ToList();
    }
}