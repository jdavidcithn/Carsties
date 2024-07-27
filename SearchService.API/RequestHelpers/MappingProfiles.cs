using AutoMapper;
using Contracts;
using SearchService.API.Models;

namespace SearchService.API.RequestHelpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<AuctionCreated, Item>();
    }
}