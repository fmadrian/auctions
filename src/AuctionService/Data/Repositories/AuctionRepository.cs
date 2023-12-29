using AuctionService.Data.Repositories.Interfaces;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data.Repositories;
public class AuctionRepository : IAuctionRepository
{
    // 1. Inject necessary dependencies
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    

    public AuctionRepository(ApplicationDbContext context, IMapper mapper){
        this._context = context;
        this._mapper = mapper;
    }
    // 2. Implement interface.
    public void AddAuction(Auction auction)
    {
        _context.Auctions.Add(auction);
    }

    public async Task<Auction> GetAuctionEntityById(Guid id)
    {
        // 1. Search auction using ID provided.
          return await this._context.Auctions
                                .Include(a => a.Item) // Include item information
                                .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<AuctionDto>> GetAuctionsAsync(string date)
    {
        var query = this._context.Auctions.OrderBy(a => a.Item.Make).AsQueryable(); //Use .AsQueryable() to add further queries.
            if (!string.IsNullOrEmpty(date))
            {
                // Only auctions that are after the date.
                query = query.Where(x => x.UpdatedAt.CompareTo(DateTimeOffset.Parse(date).ToUniversalTime()) > 0);
            }
            // Executes query, projects (maps) objects into a list of AuctionDto.
            List<AuctionDto> result = await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
        return this._mapper.Map<List<AuctionDto>>(result);
    }

    public async Task<AuctionDto> GetAuctionByIdAsync(Guid id)
    {
        return await this._context.Auctions
                                    // .Include(auction => auction.Item) // Include (information from other table) related property.
                                    .ProjectTo<AuctionDto>(_mapper.ConfigurationProvider)
                                    .FirstOrDefaultAsync(auction => auction.Id == id); // Search by exact id.
    }

    public void RemoveAuction(Auction auction)
    {
        this._context.Remove(auction);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
