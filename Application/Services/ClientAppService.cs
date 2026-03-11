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

            // Используем ITaxService для всех расчетов
            var stats = _taxService.GetTariffUsage(client, year, month);

            // Для НДС нам нужно знать порог из констант
            decimal threshold = Domain.Constants.TaxConstants.NdsThresholds.TryGetValue(
                year,
                out var t
            )
                ? t
                : Domain.Constants.TaxConstants.DefaultNdsThreshold;

            var tasks = await _context
                .Tasks.Where(t => t.ClientId == clientId && !t.IsCompleted)
                .ToListAsync();

            var monthlyTransactions = client
                .Transactions.Where(t =>
                    t.Date.Year == year && t.Date.Month == month && t.Status == "Completed"
                )
                .ToList();

            return new ClientDashboardDto
            {
                Id = client.Id,
                FirstName = client.FirstName,
                LastName = client.LastName,
                TaxRegime = client.TaxRegime,
                TaxRiskLevel = client.TaxRiskLevel,
                // Прямой маппинг из сущности Client
                PersonnelCount = client.EmployeesCount,

                // Берем данные из ITaxService, чтобы логика была в одном месте
                OperationsLimit =
                    (client.CurrentTariff?.OperationsLimit ?? 0)
                    + (client.CurrentTariff?.CarriedOverOperations ?? 0),
                OperationsActual =
                    (client.CurrentTariff?.OperationsLimit ?? 0)
                    + (client.CurrentTariff?.CarriedOverOperations ?? 0)
                    - stats.RemainingOperations,

                ConsultingMinutesLimit =
                    (client.CurrentTariff?.CommunicationMinutesLimit ?? 0)
                    + (client.CurrentTariff?.CarriedOverMinutes ?? 0),
                ConsultingMinutesActual =
                    (client.CurrentTariff?.CommunicationMinutesLimit ?? 0)
                    + (client.CurrentTariff?.CarriedOverMinutes ?? 0)
                    - stats.RemainingMinutes,

                // Отчетность по типам (ServiceType)
                StatReportsCount = monthlyTransactions.Count(t => t.ServiceType == "Stat"),
                MonthlyTaxReports = monthlyTransactions.Count(t => t.ServiceType == "TaxMonthly"),
                QuarterlyTaxReports = monthlyTransactions.Count(t =>
                    t.ServiceType == "TaxQuarterly"
                ),
                SemiAnnualTaxReports = monthlyTransactions.Count(t =>
                    t.ServiceType == "TaxSemiAnnual"
                ),
                AnnualTaxReports = monthlyTransactions.Count(t => t.ServiceType == "TaxAnnual"),

                // Финансы
                TariffAmount = client.CurrentTariff?.MonthlyFee ?? 0,
                // Сумма доп. услуг, оказанных именно в этом месяце
                MonthlyExtraServicesAmount = monthlyTransactions
                    .Where(t => t.IsExtraService)
                    .Sum(t => t.Amount),
                // Берем актуальный долг из профиля клиента
                TotalOutstandingDebt = client.TotalDebt,

                // НДС (Лист "Справочник")
                NdsThreshold = threshold,
                CurrentYearTurnover = threshold - _taxService.GetRemainingNdsLimit(client, year),

                // Сроки и задачи
                EcpExpiryDate = client.EcpExpiryDate,
                DaysUntilEcpExpires = client.EcpExpiryDate.HasValue
                    ? Math.Max(
                        0,
                        (client.EcpExpiryDate.Value.ToUniversalTime() - DateTime.UtcNow).Days
                    )
                    : 0,
                ActiveTasksCount = tasks.Count,
                OverdueTasksCount = tasks.Count(t => t.Deadline < DateTime.UtcNow),
            };
        }
    }
}
