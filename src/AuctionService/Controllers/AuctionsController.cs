using Auctions.DTOs;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<AuctionsController> _logger;
        public AuctionsController(ApplicationDbContext _applicationDbContext, IMapper _mapper, ILogger<AuctionsController> _logger)
        {
            this._applicationDbContext = _applicationDbContext;
            this._mapper = _mapper;
            this._logger = _logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
        {

            var query = _applicationDbContext.Auctions.OrderBy(a => a.Item.Make).AsQueryable(); //Use .AsQueryable() to add further queries.
            if (!string.IsNullOrEmpty(date))
            {
                // Only auctions that are after the date.
                query = query.Where(x => x.UpdatedAt.CompareTo(DateTimeOffset.Parse(date).ToUniversalTime()) > 0);
            }
            // Executes query, projects (maps) objects into a list of AuctionDto.
            List<AuctionDto> result = await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
            return this._mapper.Map<List<AuctionDto>>(result);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            Auction result = await this._applicationDbContext.Auctions
                                    .Include(auction => auction.Item) // Include (information from other table) related property.
                                    .FirstOrDefaultAsync(auction => auction.Id == id); // Search by exact id.
            if (result == null)
                return NotFound();
            return this._mapper.Map<AuctionDto>(result);

        }
        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto dto)
        {
            // 1. Map Auction.
            // Item is automatically mapped (see MappingProfiles)
            Auction auction = this._mapper.Map<Auction>(dto);
            // TODO: Add current user as seller.
            auction.Seller = "test";

            // 2. Store them into the context.
            await this._applicationDbContext.Auctions.AddAsync(auction);

            // 3. Save DB changes.
            // SaveChanges returns an integer that represents each change saved in the database. 
            bool changesSaved = await this._applicationDbContext.SaveChangesAsync() > 0;
            if (!changesSaved)
                return BadRequest("Could not save data.");
            // 4. Return an address where the created object can be found, the ID of the object, and the object.
            return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, this._mapper.Map<AuctionDto>(auction));
        }
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, [FromBody] UpdateAuctionDto dto)
        {
            // 1. Search auction using ID provided.
            Auction auction = await this._applicationDbContext.Auctions
                                            .Include(a => a.Item) // Include item information
                                            .FirstOrDefaultAsync(a => a.Id == id);

            if (auction == null)
                return NotFound();

            // TODO: Check seller name and username.

            // 2. Make changes (if the field on the DTO are not null)
            auction.Item.Make = dto.Make ?? auction.Item.Make;
            auction.Item.Model = dto.Model ?? auction.Item.Model;
            auction.Item.Color = dto.Color ?? auction.Item.Color;
            auction.Item.Mileage = dto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = dto.Year ?? auction.Item.Year;

            // 3. Store changes in database and check if they were saved.
            bool changesSaved = await this._applicationDbContext.SaveChangesAsync() > 0;
            if (!changesSaved)
                return BadRequest("Changes were not saved.");

            // 4. Return HTTP 200
            return Ok();
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            // 1. Search auction by id.
            Auction auction = await this._applicationDbContext.Auctions
                                                        .FindAsync(id);

            if (auction == null)
                return NotFound();
            // TODO: Check seller ID == username

            // 2. Delete auction from context.
            this._applicationDbContext.Auctions.Remove(auction);
            // 3. Save changes in database and check them.
            bool changesSaved = await this._applicationDbContext.SaveChangesAsync() > 0;
            if (!changesSaved)
                return BadRequest("Couldn't delete the data.");

            // 4. Return HTTP 200
            return Ok();

        }
    }
};