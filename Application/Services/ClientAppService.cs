using Application.Clients;
using Domain.Services;
using Microsoft.EntityFrameworkCore;
using Storage;

namespace Application.Services
{
    public class ClientAppService
    {
        private readonly DataContext _context;
        private readonly ITaxService _taxService;
        private readonly IOveruseService _overuseService;

        public ClientAppService(
            DataContext context,
            ITaxService taxService,
            IOveruseService overuseService
        )
        {
            _context = context;
            _taxService = taxService;
            _overuseService = overuseService;
        }

        public async Task<ClientDashboardDto> GetDashboardDataAsync(
            Guid clientId,
            int year,
            int month,
            string currentUserName,
            bool isAdmin = false
        )
        {
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

            var stats = _taxService.GetTariffUsage(client, tariff, targetYear, targetMonth);

            // --- APPLY OVERUSE ---
            if (tariff != null)
            {
                _overuseService.ApplyOveruse(tariff, stats);
                await _context.SaveChangesAsync();
            }

            var filteredTransactions = client
                .Transactions.Where(t =>
                    t.Status == "Completed"
                    && (isAllTime || (t.Date.Year == year && t.Date.Month == month))
                )
                .ToList();

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

            return new ClientDashboardDto
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
                MonthlyExtraServicesAmount = filteredTransactions
                    .Where(t => t.IsExtraService)
                    .Sum(t => t.ExtraServiceAmount),

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

                // --- OVERUSE ---
                OverusedOperations = stats.OverusedOperations,
                OverusedMinutes = stats.OverusedMinutes,
                OverusedOperationsCost = stats.OverusedOperationsCost,
                OverusedMinutesCost = stats.OverusedMinutesCost,
            };
        }
    }
}
