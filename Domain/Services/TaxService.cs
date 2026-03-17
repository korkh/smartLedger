using Domain.Constants;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Domain.Services
{
    public interface ITaxService
    {
        decimal GetRemainingNdsLimit(Client client, int year);
        TariffUsageStats GetTariffUsage(Client client, ClientTariff tariff, int year, int month);
    }

    public class TaxService : ITaxService
    {
        private readonly ILogger<TaxService> _logger;

        public TaxService(ILogger<TaxService> logger)
        {
            _logger = logger;
        }

        // ---------------------------------------------------------
        // 1. Расчёт оставшегося лимита НДС
        // ---------------------------------------------------------
        public decimal GetRemainingNdsLimit(Client client, int year)
        {
            // Если клиент уже плательщик НДС — лимит не имеет смысла
            if (client.NdsStatus == "Taxpayer")
                return 0;

            // Получаем порог НДС
            if (!TaxConstants.NdsThresholds.TryGetValue(year, out var threshold))
                threshold = TaxConstants.DefaultNdsThreshold;

            // Суммируем оборот по транзакциям, где услуга влияет на НДС
            var currentTurnover = client
                .Transactions.Where(t =>
                    t.Date.Year == year
                    && t.Status == "Completed"
                    && t.Service != null
                    && t.Service.AffectsNdsThreshold
                )
                .Sum(t => t.NdsBaseAmount);

            var remaining = threshold - currentTurnover;
            return remaining > 0 ? remaining : 0;
        }

        // ---------------------------------------------------------
        // 2. Расчёт использования тарифа + хвостов
        // ---------------------------------------------------------
        public TariffUsageStats GetTariffUsage(
            Client client,
            ClientTariff tariff,
            int year,
            int month
        )
        {
            var stats = new TariffUsageStats();

            // Фильтруем транзакции за месяц
            var monthlyTransactions = client
                .Transactions.Where(t =>
                    t.Date.Year == year && t.Date.Month == month && t.Status == "Completed"
                )
                .ToList();

            // -----------------------------------------------------
            // ОТЧЁТНОСТЬ (булевые флаги)
            // -----------------------------------------------------
            stats.StatReportsCount = monthlyTransactions.Count(t => t.IsStatReport);
            stats.MonthlyTaxReports = monthlyTransactions.Count(t => t.IsMonthlyReport);
            stats.QuarterlyTaxReports = monthlyTransactions.Count(t => t.IsQuarterlyReport);
            stats.SemiAnnualTaxReports = monthlyTransactions.Count(t => t.IsSemiAnnualReport);
            stats.AnnualTaxReports = monthlyTransactions.Count(t => t.IsAnnualReport);

            // -----------------------------------------------------
            // Если тарифа нет — возвращаем только отчётность
            // -----------------------------------------------------
            if (tariff == null)
            {
                _logger.LogWarning("No active tariff found for client {ClientId}", client.Id);
                return stats;
            }

            // -----------------------------------------------------
            // ОПЕРАЦИИ
            // -----------------------------------------------------
            int totalOpsLimit = tariff.OperationsLimit + tariff.CarriedOverOperations;
            int usedOps = monthlyTransactions.Sum(t => t.OperationsCount);

            stats.RemainingOperations = Math.Max(0, totalOpsLimit - usedOps);
            stats.OverusedOperations = Math.Max(0, usedOps - totalOpsLimit);

            // Стоимость перерасхода (можно вынести в тариф)
            stats.OverusedOperationsCost = stats.OverusedOperations * 500m;

            stats.OperationsPercentage =
                totalOpsLimit > 0 ? Math.Round((double)usedOps / totalOpsLimit * 100, 2) : 0;

            // -----------------------------------------------------
            // МИНУТЫ
            // -----------------------------------------------------
            int totalMinLimit = tariff.CommunicationMinutesLimit + tariff.CarriedOverMinutes;
            int usedMinutes = monthlyTransactions.Sum(t => t.ActualTimeMinutes);

            stats.RemainingMinutes = Math.Max(0, totalMinLimit - usedMinutes);
            stats.OverusedMinutes = Math.Max(0, usedMinutes - totalMinLimit);

            stats.OverusedMinutesCost = stats.OverusedMinutes * 50m;

            stats.MinutesPercentage =
                totalMinLimit > 0 ? Math.Round((double)usedMinutes / totalMinLimit * 100, 2) : 0;

            return stats;
        }
    }
}
