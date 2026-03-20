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
            // Если ClientDto используется как основной контейнер в Handler
            CreateMap<Client, ClientDto>().IncludeBase<Client, ClientLevel3Dto>();
            CreateMap<ClientDto, Client>()
                .ForMember(d => d.Internal, o => o.Ignore())
                .ForMember(d => d.Sensitive, o => o.Ignore());

            // --- LEVEL 1  ---
            CreateMap<Client, ClientLevel1Dto>()
                .ForMember(d => d.TaxRegime, o => o.MapFrom(s => s.TaxRegime.ToString()));
            ;

            // --- LEVEL 2  ---
            CreateMap<Client, ClientLevel2Dto>()
                .IncludeBase<Client, ClientLevel1Dto>()
                .ForMember(
                    d => d.ResponsiblePersonContact,
                    o => o.MapFrom(s => s.Internal.ResponsiblePersonContact)
                )
                .ForMember(
                    d => d.BankManagerContact,
                    o => o.MapFrom(s => s.Internal.BankManagerContact)
                )
                .ForMember(d => d.ManagerNotes, o => o.MapFrom(s => s.Internal.ManagerNotes));

            // --- LEVEL 3 (Чувствительные данные + Шифрование) ---
            CreateMap<Client, ClientLevel3Dto>()
                .IncludeBase<Client, ClientLevel2Dto>()
                .ForMember(d => d.StrategicNotes, o => o.MapFrom(s => s.Sensitive.StrategicNotes))
                .ForMember(d => d.PersonalInfo, o => o.MapFrom(s => s.Sensitive.PersonalInfo))
                .ForMember(d => d.EcpPassword, o => o.Ignore())
                .ForMember(d => d.EsfPassword, o => o.Ignore())
                .ForMember(d => d.BankingPasswords, o => o.Ignore());

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
