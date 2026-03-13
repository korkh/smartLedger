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
            CreateMap<ClientDto, Client>()
                // Не обновляем эти поля, если они пришли пустыми (защита данных 3-го уровня)
                .ForMember(
                    dest => dest.StrategicNotes,
                    opt => opt.Condition(src => src.StrategicNotes != null)
                )
                .ForMember(
                    dest => dest.PersonalInfo,
                    opt => opt.Condition(src => src.PersonalInfo != null)
                )
                // Остальные поля
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // 2. Transaction Mapping
            CreateMap<Transaction, TransactionDto>()
                .ForMember(
                    dest => dest.ClientName,
                    opt => opt.MapFrom(src => $"{src.Client.FirstName} {src.Client.LastName}")
                )
                .ForMember(
                    dest => dest.ServiceTypeName,
                    opt => opt.MapFrom(src => src.Service.Name)
                )
                .ReverseMap();

            // 3. Dashboard Mapping
            CreateMap<Client, ClientDashboardDto>()
                .ForMember(d => d.TariffAmount, o => o.MapFrom(s => s.CurrentTariff.MonthlyFee))
                .ForMember(
                    d => d.OperationsLimit,
                    o => o.MapFrom(s => s.CurrentTariff.OperationsLimit)
                );

            // 4. ClientTariff Mapping
            CreateMap<ClientTariff, ClientTariffDto>()
                .ForMember(d => d.ContractAmount, o => o.MapFrom(s => s.MonthlyFee))
                .ForMember(d => d.AllowedOperations, o => o.MapFrom(s => s.OperationsLimit))
                .ForMember(
                    d => d.AllowedCommunicationMinutes,
                    o => o.MapFrom(s => s.CommunicationMinutesLimit)
                )
                .ForMember(d => d.StartDate, o => o.MapFrom(s => s.ContractDate))
                .ReverseMap();
        }
    }
}
