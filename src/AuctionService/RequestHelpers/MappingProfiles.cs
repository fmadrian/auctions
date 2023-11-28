using Auctions.DTOs;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;

namespace AuctionService.RequestHelpers;
public class MappingProfiles : Profile
{
    public MappingProfiles()
    {

        /*
            We're mapping objects from 2 entities into DTO and viceversa.

            We need to Map
                    CreateDTO => Entity1
                    CreateDTO => Entity2
                    Entity1 => ResponseDTO
                    Entity2 => ResponseDTO
        
        */

        // 1. Map Auction into AuctionDto
        // 2. Include Item object into mapping.
        CreateMap<Auction, AuctionDto>().IncludeMembers(auction => auction.Item);
        CreateMap<Item, AuctionDto>();

        // 1. Map CreateAuction into AuctionDto.
        CreateMap<CreateAuctionDto, Auction>()
            .ForMember(dto => dto.Item, options => options.MapFrom(source => source));
        // 2. Create related mapping.
        CreateMap<CreateAuctionDto, Item>();

        // 3. Map AuctionDto into AuctionCreated
        CreateMap<AuctionDto, AuctionCreated>();

        // 4. Map Auction into AuctionUpdated
        CreateMap<Auction, AuctionUpdated>().IncludeMembers(a => a.Item); // Include Item object included on the entity.
        // REMEMBER: Include the extra mapping.
        CreateMap<Item, AuctionUpdated>();

    }
}
