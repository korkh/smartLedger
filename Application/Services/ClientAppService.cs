using Application.Clients;
using Domain.Services;
using Microsoft.EntityFrameworkCore;
using Storage;

namespace Application.Services
{
    public class ClientAppService(DataContext context, ITaxService taxService)
    {
        private readonly DataContext _context = context;
        private readonly ITaxService _taxService = taxService;

        public async Task<ClientDashboardDto> GetDashboardDataAsync(
            Guid clientId,
            int year,
            int month,
            string currentUserName,
            bool isAdmin = false
        )
        {
            var query = _context
                .Clients.Include(c => c.CurrentTariff)
                .Include(c => c.Transactions)
                    .ThenInclude(t => t.Service)
                .AsQueryable();

            if (!isAdmin)
                query = query.Where(c => c.ResponsiblePersonContact.Contains(currentUserName));

            var client = await query.FirstOrDefaultAsync(c => c.Id == clientId);
            if (client == null)
                return null;

            bool isAllTime = year <= 0 || month <= 0;
            int targetYear = isAllTime ? DateTime.Now.Year : year;
            int targetMonth = isAllTime ? DateTime.Now.Month : month;

            // ITaxService теперь является единственным источником правды для суммирования показателей
            var stats = _taxService.GetTariffUsage(client, targetYear, targetMonth);

            // Фильтрация транзакций для финансовых расчетов и НДС
            var filteredTransactions = client
                .Transactions.Where(t =>
                    t.Status == "Completed"
                    && (isAllTime || (t.Date.Year == year && t.Date.Month == month))
                )
                .ToList();

            // Расчет порога НДС
            int ndsYear = targetYear;
            decimal threshold = Domain.Constants.TaxConstants.NdsThresholds.TryGetValue(
                ndsYear,
                out var tLimit
            )
                ? tLimit
                : Domain.Constants.TaxConstants.DefaultNdsThreshold;

            var tasks = await _context.Tasks.Where(t => t.ClientId == clientId).ToListAsync();
            var now = DateTime.UtcNow;

            return new ClientDashboardDto
            {
                Id = client.Id,
                FirstName = client.FirstName,
                LastName = client.LastName,
                TaxRegime = client.TaxRegime,
                TaxRiskLevel = client.TaxRiskLevel,
                PersonnelCount = client.EmployeesCount,

                // --- OPERATIONS ---
                OperationsLimit =
                    (client.CurrentTariff?.OperationsLimit ?? 0)
                    + (client.CurrentTariff?.CarriedOverOperations ?? 0),
                OperationsActual = isAllTime
                    ? client
                        .Transactions.Where(t => t.Status == "Completed")
                        .Sum(t => t.OperationsCount)
                    : (
                        (client.CurrentTariff?.OperationsLimit ?? 0)
                        + (client.CurrentTariff?.CarriedOverOperations ?? 0)
                    ) - stats.RemainingOperations,

                ConsultingMinutesLimit =
                    (client.CurrentTariff?.CommunicationMinutesLimit ?? 0)
                    + (client.CurrentTariff?.CarriedOverMinutes ?? 0),
                ConsultingMinutesActual = isAllTime
                    ? client
                        .Transactions.Where(t => t.Status == "Completed")
                        .Sum(t => t.ActualTimeMinutes)
                    : (
                        (client.CurrentTariff?.CommunicationMinutesLimit ?? 0)
                        + (client.CurrentTariff?.CarriedOverMinutes ?? 0)
                    ) - stats.RemainingMinutes,

                // --- REPORTING (Trusting TaxService mapping) ---
                // We use stats directly as it already contains summed int values for reports
                StatReportsCount = stats.StatReportsCount,
                MonthlyTaxReports = stats.MonthlyTaxReports,
                QuarterlyTaxReports = stats.QuarterlyTaxReports,
                SemiAnnualTaxReports = stats.SemiAnnualTaxReports,
                AnnualTaxReports = stats.AnnualTaxReports,

                // --- FINANCES ---
                TariffAmount = client.CurrentTariff?.MonthlyFee ?? 0,
                MonthlyExtraServicesAmount = filteredTransactions
                    .Where(t => t.IsExtraService)
                    .Sum(t => t.ExtraServiceAmount),
                TotalOutstandingDebt = client.TotalDebt,

                // --- VAT (NDS) ---
                NdsThreshold = threshold,
                CurrentYearTurnover = threshold - _taxService.GetRemainingNdsLimit(client, ndsYear),
                MonthlyTurnover = filteredTransactions.Sum(t => t.NdsBaseAmount),

                // --- DEADLINES & TASKS ---
                EcpExpiryDate = client.EcpExpiryDate,
                DaysUntilEcpExpires = client.EcpExpiryDate.HasValue
                    ? Math.Max(0, (client.EcpExpiryDate.Value.ToUniversalTime() - now).Days)
                    : 0,
                ActiveTasksCount = tasks.Count(t => !t.IsCompleted),
                OverdueTasksCount = tasks.Count(t => !t.IsCompleted && t.Deadline < now),
            };
        }
    }
}
