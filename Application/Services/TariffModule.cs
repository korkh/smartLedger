using Application.Clients;
using Application.Interfaces;
using Domain.Entities;
using Domain.Services;
using Microsoft.EntityFrameworkCore;
using Storage;

namespace Application.Services
{
    public interface ITariffModule
    {
        Task<ClientDashboardDto> BuildDashboardAsync(
            Guid clientId,
            int year,
            int month,
            string currentUserName,
            bool isAdmin = false
        );
    }

    public class TariffModule : ITariffModule
    {
        private readonly DataContext _context;
        private readonly ITaxService _taxService;
        private readonly IOveruseService _overuseService;
        private readonly IBillingService _billingService;
        private readonly IAiAnalysisService _aiService;
        private readonly IAiRiskAnalysisService _riskService;
        private readonly IRiskAlertService _riskAlertService;

        public TariffModule(
            DataContext context,
            ITaxService taxService,
            IOveruseService overuseService,
            IBillingService billingService,
            IAiAnalysisService aiService,
            IAiRiskAnalysisService riskService,
            IRiskAlertService riskAlertService
        )
        {
            _context = context;
            _taxService = taxService;
            _overuseService = overuseService;
            _billingService = billingService;
            _aiService = aiService;
            _riskService = riskService;
            _riskAlertService = riskAlertService;
        }

        public async Task<ClientDashboardDto> BuildDashboardAsync(
            Guid clientId,
            int year,
            int month,
            string currentUserName,
            bool isAdmin = false
        )
        {
            // --- LOAD CLIENT ---
            var query = _context
                .Clients.Include(c => c.ClientTariffs)
                .Include(c => c.Transactions)
                    .ThenInclude(t => t.Service)
                .Include(c => c.Internal)
                .AsQueryable();

            if (!isAdmin)
                query = query.Where(c =>
                    c.Internal != null
                    && c.Internal.ResponsiblePersonContact.Contains(currentUserName)
                );

            var client = await query.FirstOrDefaultAsync(c => c.Id == clientId);
            if (client == null)
                return null;

            var tariff = client.CurrentTariff;

            bool isAllTime = year <= 0 || month <= 0;
            int targetYear = isAllTime ? DateTime.Now.Year : year;
            int targetMonth = isAllTime ? DateTime.Now.Month : month;

            // --- CALCULATE TARIFF USAGE ---
            var stats = _taxService.GetTariffUsage(client, tariff, targetYear, targetMonth);

            // --- APPLY OVERUSE (ХВОСТЫ) ---
            if (tariff != null)
                _overuseService.ApplyOveruse(tariff, stats);

            // --- FILTER TRANSACTIONS ---
            var filteredTransactions = client
                .Transactions.Where(t =>
                    t.Status == "Completed"
                    && (isAllTime || (t.Date.Year == year && t.Date.Month == month))
                )
                .ToList();

            decimal extraServicesAmount = filteredTransactions
                .Where(t => t.IsExtraService)
                .Sum(t => t.ExtraServiceAmount);

            // --- VAT ---
            int ndsYear = targetYear;
            decimal threshold = Domain.Constants.TaxConstants.NdsThresholds.TryGetValue(
                ndsYear,
                out var tLimit
            )
                ? tLimit
                : Domain.Constants.TaxConstants.DefaultNdsThreshold;

            var tasks = await _context.Tasks.Where(t => t.ClientId == clientId).ToListAsync();
            var now = DateTime.UtcNow;

            int opsLimit = (tariff?.OperationsLimit ?? 0) + (tariff?.CarriedOverOperations ?? 0);
            int minLimit =
                (tariff?.CommunicationMinutesLimit ?? 0) + (tariff?.CarriedOverMinutes ?? 0);

            // --- BUILD DASHBOARD DTO ---
            var dto = new ClientDashboardDto
            {
                Id = client.Id,
                FirstName = client.FirstName,
                LastName = client.LastName,
                TaxRegime = client.TaxRegime.ToString(),
                TaxRiskLevel = client.TaxRiskLevel,
                PersonnelCount = client.EmployeesCount,

                OperationsLimit = opsLimit,
                OperationsActual = isAllTime
                    ? client
                        .Transactions.Where(t => t.Status == "Completed")
                        .Sum(t => t.OperationsCount)
                    : opsLimit - stats.RemainingOperations,

                ConsultingMinutesLimit = minLimit,
                ConsultingMinutesActual = isAllTime
                    ? client
                        .Transactions.Where(t => t.Status == "Completed")
                        .Sum(t => t.ActualTimeMinutes)
                    : minLimit - stats.RemainingMinutes,

                StatReportsCount = stats.StatReportsCount,
                MonthlyTaxReports = stats.MonthlyTaxReports,
                QuarterlyTaxReports = stats.QuarterlyTaxReports,
                SemiAnnualTaxReports = stats.SemiAnnualTaxReports,
                AnnualTaxReports = stats.AnnualTaxReports,

                TariffAmount = tariff?.MonthlyFee ?? 0,
                MonthlyExtraServicesAmount = extraServicesAmount,
                TotalOutstandingDebt = client.TotalDebt,

                NdsThreshold = threshold,
                CurrentYearTurnover = threshold - _taxService.GetRemainingNdsLimit(client, ndsYear),
                MonthlyTurnover = filteredTransactions.Sum(t => t.NdsBaseAmount),

                EcpExpiryDate = client.EcpExpiryDate,
                DaysUntilEcpExpires = client.EcpExpiryDate.HasValue
                    ? Math.Max(0, (client.EcpExpiryDate.Value.ToUniversalTime() - now).Days)
                    : 0,

                ActiveTasksCount = tasks.Count(t => !t.IsCompleted),
                OverdueTasksCount = tasks.Count(t => !t.IsCompleted && t.Deadline < now),

                OverusedOperations = stats.OverusedOperations,
                OverusedMinutes = stats.OverusedMinutes,
                OverusedOperationsCost = stats.OverusedOperationsCost,
                OverusedMinutesCost = stats.OverusedMinutesCost,
            };

            // --- BILLING ---
            var invoice = _billingService.CreateMonthlyInvoice(
                client,
                tariff,
                stats,
                extraServicesAmount,
                targetYear,
                targetMonth
            );
            _context.Invoices.Add(invoice);

            // --- HISTORY ---
            var history = new TariffHistory
            {
                Id = Guid.NewGuid(),
                ClientId = client.Id,
                TariffId = tariff?.Id ?? Guid.Empty,
                Year = targetYear,
                Month = targetMonth,
                UsedOperations = dto.OperationsActual,
                UsedMinutes = dto.ConsultingMinutesActual,
                OverusedOperations = stats.OverusedOperations,
                OverusedMinutes = stats.OverusedMinutes,
                OverusedOperationsCost = stats.OverusedOperationsCost,
                OverusedMinutesCost = stats.OverusedMinutesCost,
                TariffAmount = dto.TariffAmount,
                ExtraServicesAmount = extraServicesAmount,
                TotalToPay = dto.TotalToPay,
            };
            _context.TariffHistories.Add(history);

            await _context.SaveChangesAsync();

            // --- AI INSIGHT ---
            dto.AiInsight = await _aiService.GetDashboardInsightAsync(
                new
                {
                    clientId = dto.Id,
                    firstName = dto.FirstName,
                    lastName = dto.LastName,
                    taxRegime = dto.TaxRegime,
                    taxRiskLevel = dto.TaxRiskLevel,
                    operationsLimit = dto.OperationsLimit,
                    operationsActual = dto.OperationsActual,
                    consultingMinutesLimit = dto.ConsultingMinutesLimit,
                    consultingMinutesActual = dto.ConsultingMinutesActual,
                    statReportsCount = dto.StatReportsCount,
                    monthlyTaxReports = dto.MonthlyTaxReports,
                    quarterlyTaxReports = dto.QuarterlyTaxReports,
                    semiAnnualTaxReports = dto.SemiAnnualTaxReports,
                    annualTaxReports = dto.AnnualTaxReports,
                    tariffAmount = dto.TariffAmount,
                    monthlyExtraServicesAmount = dto.MonthlyExtraServicesAmount,
                    totalOutstandingDebt = dto.TotalOutstandingDebt,
                    totalToPay = dto.TotalToPay,
                    ndsThreshold = dto.NdsThreshold,
                    monthlyTurnover = dto.MonthlyTurnover,
                    currentYearTurnover = dto.CurrentYearTurnover,
                    daysUntilEcpExpires = dto.DaysUntilEcpExpires,
                    activeTasksCount = dto.ActiveTasksCount,
                    overdueTasksCount = dto.OverdueTasksCount,
                }
            );

            // --- AI RISK ANALYSIS (STRUCTURED) ---
            var risk = await _riskService.AnalyzeRiskStructuredAsync(
                new
                {
                    operationsActual = dto.OperationsActual,
                    operationsLimit = dto.OperationsLimit,
                    currentYearTurnover = dto.CurrentYearTurnover,
                    ndsThreshold = dto.NdsThreshold,
                    totalOutstandingDebt = dto.TotalOutstandingDebt,
                    overdueTasksCount = dto.OverdueTasksCount,
                    daysUntilEcpExpires = dto.DaysUntilEcpExpires,
                }
            );

            // Fill DTO
            dto.RiskInsight = risk.Summary;
            dto.RiskScore = risk.RiskScore;
            dto.RiskLevel = risk.RiskLevel;
            dto.RiskColor = risk.RiskColor;
            dto.RiskRecommendations = risk.Recommendations;

            // Fill Risk metrics in history
            history.RiskScore = risk.RiskScore;
            history.RiskLevel = risk.RiskLevel;
            history.RiskColor = risk.RiskColor;
            history.RiskRecommendations = risk.Recommendations;

            // --- AI RISK FORECAST ---
            dto.RiskForecast = await _riskService.ForecastRiskAsync(
                new
                {
                    currentYearTurnover = dto.CurrentYearTurnover,
                    totalOutstandingDebt = dto.TotalOutstandingDebt,
                    overdueTasksCount = dto.OverdueTasksCount,
                    operationsActual = dto.OperationsActual,
                    operationsLimit = dto.OperationsLimit,
                }
            );

            // --- RISK ALERTS ---
            await _riskAlertService.CheckAndNotifyAsync(client, risk);

            return dto;
        }
    }
}
