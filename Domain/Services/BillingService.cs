// Domain/Services/IBillingService.cs
using Domain.Entities;

namespace Domain.Services
{
    public interface IBillingService
    {
        Invoice CreateMonthlyInvoice(
            Client client,
            ClientTariff tariff,
            TariffUsageStats stats,
            decimal extraServicesAmount,
            int year,
            int month
        );
    }

    public class BillingService : IBillingService
    {
        public Invoice CreateMonthlyInvoice(
            Client client,
            ClientTariff tariff,
            TariffUsageStats stats,
            decimal extraServicesAmount,
            int year,
            int month
        )
        {
            return new Invoice
            {
                ClientId = client.Id,
                Year = year,
                Month = month,
                TariffAmount = tariff?.MonthlyFee ?? 0,
                ExtraServicesAmount = extraServicesAmount,
                OveruseAmount = stats.OverusedOperationsCost + stats.OverusedMinutesCost,
                IsPaid = false,
            };
        }
    }
}
