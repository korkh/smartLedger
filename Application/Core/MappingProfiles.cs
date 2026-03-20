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
            // --- CLIENT (LEVEL 1–3 DTO) ---
            CreateMap<Client, ClientDto>()
                .ForMember(d => d.TaxRegime, o => o.MapFrom(s => s.TaxRegime.ToString()))
                .ForMember(d => d.StrategicNotes, o => o.MapFrom(s => s.Sensitive.StrategicNotes))
                .ForMember(d => d.PersonalInfo, o => o.MapFrom(s => s.Sensitive.PersonalInfo))
                .ReverseMap()
                .ForMember(d => d.Sensitive, o => o.Ignore()) // Sensitive маппим вручную
                .ForAllMembers(opts =>
                {
                    opts.Condition(
                        (src, dest, srcMember, context) =>
                        {
                            // Если это Level 3 поле — пропускаем
                            if (opts.DestinationMember.Name is "StrategicNotes" or "PersonalInfo")
                                return false;

                            return srcMember != null;
                        }
                    );
                });

            // --- TRANSACTION ---
            CreateMap<Transaction, TransactionDto>()
                .ForMember(
                    d => d.ClientName,
                    o => o.MapFrom(s => s.Client.FirstName + " " + s.Client.LastName)
                )
                .ForMember(d => d.ServiceCategoryName, o => o.MapFrom(s => s.Service.Name))
                .ForMember(d => d.ServiceCategory, o => o.MapFrom(s => s.Category))
                .ReverseMap()
                .ForMember(d => d.Client, o => o.Ignore())
                .ForMember(d => d.Service, o => o.Ignore());

            // --- DASHBOARD ---
            CreateMap<Client, ClientDashboardDto>()
                .ForMember(d => d.TariffAmount, o => o.MapFrom(s => s.CurrentTariff.MonthlyFee))
                .ForMember(
                    d => d.OperationsLimit,
                    o => o.MapFrom(s => s.CurrentTariff.OperationsLimit)
                );

            // --- TARIFF ---
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
