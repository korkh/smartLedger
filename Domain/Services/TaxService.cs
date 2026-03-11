using Domain.Constants;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Domain.Services;

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

    // Метрики для компонентов <ReportMetric />
    public int StatReportsCount { get; set; }
    public int MonthlyTaxReports { get; set; }
    public int QuarterlyTaxReports { get; set; }
    public int SemiAnnualTaxReports { get; set; }
    public int AnnualTaxReports { get; set; }
}

public class TaxService(ILogger<TaxService> logger) : ITaxService
{
    public decimal GetRemainingNdsLimit(Client client, int year)
    {
        // If already a VAT taxpayer, the limit is irrelevant
        if (client.NdsStatus == "Taxpayer")
            return 0;

        if (!TaxConstants.NdsThresholds.TryGetValue(year, out var threshold))
            threshold = TaxConstants.DefaultNdsThreshold;

        // Summing the turnover based on transactions that affect the VAT threshold
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

    public TariffUsageStats GetTariffUsage(Client client, int year, int month)
    {
        var stats = new TariffUsageStats();

        // Filtering transactions for the specific month and year
        var monthlyTransactions = client
            .Transactions.Where(t =>
                t.Date.Year == year && t.Date.Month == month && t.Status == "Completed"
            )
            .ToList();

        // 1. Map report metrics from transaction properties (aligned with CSV columns)
        stats.StatReportsCount = monthlyTransactions.Sum(t => t.StatReports ?? 0);
        // В TaxService.cs
        stats.MonthlyTaxReports = monthlyTransactions.Sum(t =>
            t.MonthlyTaxReports.GetValueOrDefault()
        );
        stats.QuarterlyTaxReports = monthlyTransactions.Sum(t =>
            t.QuarterlyTaxReports.GetValueOrDefault()
        );
        stats.SemiAnnualTaxReports = monthlyTransactions.Sum(t => t.SemiAnnualTaxReports ?? 0);
        stats.AnnualTaxReports = monthlyTransactions.Sum(t => t.AnnualTaxReports ?? 0);

        var tariff = client.CurrentTariff;
        if (tariff == null)
        {
            logger.LogWarning("No tariff found for client {ClientId}", client.Id);
            return stats;
        }

        // 2. Operations calculation (Limit + Carry-over vs Actual used)
        int totalOpsLimit = tariff.OperationsLimit + tariff.CarriedOverOperations;
        int usedOps = monthlyTransactions.Sum(t => t.OperationsCount);

        stats.RemainingOperations = Math.Max(0, totalOpsLimit - usedOps);
        stats.OperationsPercentage =
            totalOpsLimit > 0 ? Math.Round((double)usedOps / totalOpsLimit * 100, 2) : 0;

        // 3. Communication minutes calculation
        int totalMinLimit = tariff.CommunicationMinutesLimit + tariff.CarriedOverMinutes;
        int usedMinutes = monthlyTransactions.Sum(t => t.ActualTimeMinutes);

        stats.RemainingMinutes = Math.Max(0, totalMinLimit - usedMinutes);
        stats.MinutesPercentage =
            totalMinLimit > 0 ? Math.Round((double)usedMinutes / totalMinLimit * 100, 2) : 0;

        return stats;
    }
}
