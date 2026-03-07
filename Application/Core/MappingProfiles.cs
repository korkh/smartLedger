using Application.Clients;
using Application.Transactions;
using AutoMapper;
using Domain.Entities;

namespace Application.Core
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // 1. Client Mapping
            CreateMap<Client, ClientDto>()
                .ReverseMap()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // 2. Transaction Mapping
            CreateMap<Transaction, TransactionDto>()
                .ForMember(
                    dest => dest.ClientName,
                    opt => opt.MapFrom(src => $"{src.Client.FirstName} {src.Client.LastName}")
                )
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.Name))
                .ReverseMap();

            // 3. Dashboard Mapping
            CreateMap<Client, ClientDashboardDto>()
                // Guid мапится автоматически, если имена совпадают
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));

            // 4. ClientTariff
            CreateMap<ClientTariff, ClientTariffDto>().ReverseMap();
        }
    }
}
