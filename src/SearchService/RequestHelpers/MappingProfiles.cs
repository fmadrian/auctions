using AutoMapper;
using Contracts;
using SearchService.Entities;

namespace SearchService.RequestHelpers;
public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<AuctionCreated, Item>();
        // Map an AuctionUpdated event into an item
        CreateMap<AuctionUpdated, Item>();
    }
}
