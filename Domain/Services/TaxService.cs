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
            if (client.NdsStatus == "Плательщик НДС")
                return 0;

            if (!TaxConstants.NdsThresholds.TryGetValue(year, out var threshold))
                threshold = TaxConstants.DefaultNdsThreshold;

            var totalIncome = client
                .Transactions.Where(t =>
                    t.Date.Year == year
                    && t.Status == "Завершен"
                    && t.Service?.AffectsNdsThreshold == true
                )
                .Sum(t => t.ExtraServiceAmount);

            var remaining = threshold - totalIncome;
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
                    t.Date.Year == year && t.Date.Month == month && t.Status == "Завершен"
                )
                .ToList();

            // Calculate operations
            int totalOperationsUsed = monthlyTransactions.Sum(t => t.OperationsCount);
            int totalOperationsLimit = tariff.OperationsLimit + tariff.CarriedOverOperations;

            stats.RemainingOperations = Math.Max(0, totalOperationsLimit - totalOperationsUsed);
            stats.OperationsPercentage =
                totalOperationsLimit > 0
                    ? (double)totalOperationsUsed / totalOperationsLimit * 100
                    : 0;

            // Calculate communication minutes - FIXED PROPERTY NAME HERE
            int totalMinutesUsed = monthlyTransactions.Sum(t => t.CommunicationTimeMinutes);
            int totalMinutesLimit = tariff.CommunicationMinutesLimit + tariff.CarriedOverMinutes;

            stats.RemainingMinutes = Math.Max(0, totalMinutesLimit - totalMinutesUsed);
            stats.MinutesPercentage =
                totalMinutesLimit > 0 ? (double)totalMinutesUsed / totalMinutesLimit * 100 : 0;

            return stats;
        }
    }
}
