using Domain.Constants;
using Domain.Entities;

namespace Domain.Services
{
    public interface ITaxService
    {
        decimal GetRemainingNdsLimit(Client client, int year);
        TariffUsageStats GetTariffUsage(Client client, int year, int month);
    }

    public class TariffUsageStats
    {
        public int RemainingOperations { get; set; }
        public double OperationsPercentage { get; set; }
        public int RemainingMinutes { get; set; }
        public double MinutesPercentage { get; set; }
    }

    public class TaxService : ITaxService
    {
        public decimal GetRemainingNdsLimit(Client client, int year)
        {
            // Если клиент уже на НДС, лимита нет
            if (client.NdsStatus == "Taxpayer")
                return 0;

            if (!TaxConstants.NdsThresholds.TryGetValue(year, out var threshold))
                threshold = TaxConstants.DefaultNdsThreshold;

            // Суммируем только те услуги, которые влияют на порог НДС
            var currentTurnover = client
                .Transactions.Where(t =>
                    t.Date.Year == year
                    && t.Status == "Completed"
                    && t.Service != null
                    && t.Service.AffectsNdsThreshold
                )
                .Sum(t => t.ExtraServiceAmount);

            var remaining = threshold - currentTurnover;
            return remaining > 0 ? remaining : 0;
        }

        public TariffUsageStats GetTariffUsage(Client client, int year, int month)
        {
            var stats = new TariffUsageStats();
            var tariff = client.CurrentTariff;

            if (tariff == null)
                return stats;

            var monthlyTransactions = client
                .Transactions.Where(t =>
                    t.Date.Year == year && t.Date.Month == month && t.Status == "Completed"
                )
                .ToList();

            // 1. Расчет операций
            int usedOps = monthlyTransactions.Sum(t => t.OperationsCount);
            int totalOpsLimit = tariff.OperationsLimit + tariff.CarriedOverOperations;

            stats.RemainingOperations = Math.Max(0, totalOpsLimit - usedOps);
            stats.OperationsPercentage =
                totalOpsLimit > 0 ? Math.Min(100, (double)usedOps / totalOpsLimit * 100) : 0;

            // 2. Расчет минут (консультации/коммуникации)
            // Используем ActualTimeMinutes для транзакций типа "Консультация"
            int usedMinutes = monthlyTransactions.Sum(t => t.ActualTimeMinutes);
            int totalMinLimit = tariff.CommunicationMinutesLimit + tariff.CarriedOverMinutes;

            stats.RemainingMinutes = Math.Max(0, totalMinLimit - usedMinutes);
            stats.MinutesPercentage =
                totalMinLimit > 0 ? Math.Min(100, (double)usedMinutes / totalMinLimit * 100) : 0;

            return stats;
        }
    }
}
