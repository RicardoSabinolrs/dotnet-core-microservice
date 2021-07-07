using AutoMapper;
using SabinoLabs.Domain.Dto;
using SabinoLabs.Domain.Entities;

namespace SabinoLabs.Configuration.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile() => CreateMap<Beer, BeerDto>().ReverseMap();
    }
}
