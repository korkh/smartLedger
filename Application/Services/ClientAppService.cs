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

            var stats = _taxService.GetTariffUsage(
                client,
                isAllTime ? DateTime.Now.Year : year,
                isAllTime ? DateTime.Now.Month : month
            );

            // Фильтрация транзакций
            var filteredTransactionsQuery = client.Transactions.Where(t => t.Status == "Completed");
            if (!isAllTime)
            {
                filteredTransactionsQuery = filteredTransactionsQuery.Where(t =>
                    t.Date.Year == year && t.Date.Month == month
                );
            }

            var filteredTransactions = filteredTransactionsQuery.ToList();

            // Расчет НДС
            int ndsYear = isAllTime ? DateTime.Now.Year : year;
            decimal threshold = Domain.Constants.TaxConstants.NdsThresholds.TryGetValue(
                ndsYear,
                out var tLimit
            )
                ? tLimit
                : Domain.Constants.TaxConstants.DefaultNdsThreshold;

            // Получаем задачи для счетчиков
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

                // --- ОПЕРАЦИИ ---
                OperationsLimit =
                    (client.CurrentTariff?.OperationsLimit ?? 0)
                    + (client.CurrentTariff?.CarriedOverOperations ?? 0),
                OperationsActual = isAllTime
                    ? client
                        .Transactions.Where(t => t.Status == "Completed")
                        .Sum(t => t.OperationsCount)
                    : (client.CurrentTariff?.OperationsLimit ?? 0)
                        + (client.CurrentTariff?.CarriedOverOperations ?? 0)
                        - stats.RemainingOperations,

                ConsultingMinutesLimit =
                    (client.CurrentTariff?.CommunicationMinutesLimit ?? 0)
                    + (client.CurrentTariff?.CarriedOverMinutes ?? 0),
                ConsultingMinutesActual = isAllTime
                    ? client
                        .Transactions.Where(t => t.Status == "Completed")
                        .Sum(t => t.ActualTimeMinutes)
                    : (client.CurrentTariff?.CommunicationMinutesLimit ?? 0)
                        + (client.CurrentTariff?.CarriedOverMinutes ?? 0)
                        - stats.RemainingMinutes,

                // --- ОТЧЕТНОСТЬ (Наполняем данными из DTO) ---
                StatReportsCount = filteredTransactions.Count(t => t.ServiceType == "Stat"),
                MonthlyTaxReports = filteredTransactions.Count(t => t.ServiceType == "TaxMonthly"),
                QuarterlyTaxReports = filteredTransactions.Count(t =>
                    t.ServiceType == "TaxQuarterly"
                ),
                SemiAnnualTaxReports = filteredTransactions.Count(t =>
                    t.ServiceType == "TaxSemiAnnual"
                ), // Добавлено
                AnnualTaxReports = filteredTransactions.Count(t => t.ServiceType == "TaxAnnual"), // Добавлено

                // --- ФИНАНСЫ ---
                TariffAmount = client.CurrentTariff?.MonthlyFee ?? 0,
                MonthlyExtraServicesAmount = filteredTransactions
                    .Where(t => t.IsExtraService)
                    .Sum(t => t.ExtraServiceAmount),
                TotalOutstandingDebt = client.TotalDebt,

                // --- НДС ---
                NdsThreshold = threshold,
                CurrentYearTurnover = threshold - _taxService.GetRemainingNdsLimit(client, ndsYear),
                MonthlyTurnover = filteredTransactions.Sum(t => t.NdsBaseAmount), // Оборот за выбранный период

                // --- СРОКИ И ЗАДАЧИ ---
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
