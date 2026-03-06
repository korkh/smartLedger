using Application.Clients;
using AutoMapper;
using Domain.Entitites;

namespace Application.Core
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Client, ClientDto>();
        }
    }
}
