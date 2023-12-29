using AuctionService.Data.Repositories.Interfaces;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AuctionsController> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        public AuctionsController(IAuctionRepository auctionRepository, IMapper mapper, ILogger<AuctionsController> logger, IPublishEndpoint publishEndpoint)
        {
            this._auctionRepository = auctionRepository;
            this._mapper = mapper;
            this._logger = logger;
            this._publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
        {
            return await this._auctionRepository.GetAuctionsAsync(date);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            AuctionDto result = await this._auctionRepository.GetAuctionByIdAsync(id);
            if (result == null)
                return NotFound();
            return result;
        }
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto dto)
        {
            // 1. Map Auction.
            // Item is automatically mapped (see MappingProfiles)
            Auction auction = this._mapper.Map<Auction>(dto);
            // User.Identity.Name returns the username claim in the JWT.
            auction.Seller = User.Identity.Name; // TokenValidationParameters.NameClaimType == "username" 
                                                 // retrieves username claim as User.Identity.Name.

            /*
            *  When the outbox is configured
            *  2, 5, and 6 become an ACID transaction. 
            *  They all happen as a unit, or none of them happen at all.
            */
            // 2. Store them into the context.
            this._auctionRepository.AddAuction(auction);
            // 5. Map the saved entity into a DTO.
            AuctionDto newAuctionDto = this._mapper.Map<AuctionDto>(auction);
            // 6. Publish event/contract into service bus.
            await this._publishEndpoint.Publish(this._mapper.Map<AuctionCreated>(newAuctionDto));

            // 3. Save DB changes.
            // SaveChanges returns an integer that represents each change saved in the database. 
            bool changesSaved = await this._auctionRepository.SaveChangesAsync();
            if (!changesSaved)
                return BadRequest("Could not save data.");

            // 4. Return an address where the created object can be found, the ID of the object, and the object.
            return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, newAuctionDto);
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, [FromBody] UpdateAuctionDto dto)
        {
            Auction auction = await this._auctionRepository.GetAuctionEntityById(id);
            if (auction == null)
                return NotFound();

            // Only who created the auction is allowed to update it.
            if (auction.Seller != User.Identity.Name) return Forbid(); // Not allowed to update this auction.

            // 2. Make changes (if the field on the DTO are not null)
            auction.Item.Make = dto.Make ?? auction.Item.Make;
            auction.Item.Model = dto.Model ?? auction.Item.Model;
            auction.Item.Color = dto.Color ?? auction.Item.Color;
            auction.Item.Mileage = dto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = dto.Year ?? auction.Item.Year;

            // 5. Send a message to the service bus.
            // 5.1. Map Entity to Event and publish it onto the service bus.
            await this._publishEndpoint.Publish(this._mapper.Map<AuctionUpdated>(auction));

            // 3. Store changes in database and check if they were saved.
            bool changesSaved = await this._auctionRepository.SaveChangesAsync();
            if (!changesSaved)
                return BadRequest("Changes were not saved.");

            // 4. Return HTTP 200
            return Ok();
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            // 1. Search auction by id.
            Auction auction = await this._auctionRepository.GetAuctionEntityById(id);

            if (auction == null)
                return NotFound();
            // Only who created the auction is allowed to delete it.
            if (auction.Seller != User.Identity.Name) return Forbid(); // Not allowed to delete this auction.

            // 2. Delete auction from context.
            this._auctionRepository.RemoveAuction(auction);
            // 5. Send/publish message to the service bus.
            // No mapping involved, will send an anoynmous object that only has the ID of the auction.
            await this._publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });
            // 3. Save changes in database and check them.
            bool changesSaved = await this._auctionRepository.SaveChangesAsync();
            if (!changesSaved)
                return BadRequest("Couldn't delete the data.");

            // 4. Return HTTP 200
            return Ok();

        }
    }
};