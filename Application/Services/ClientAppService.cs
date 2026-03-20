using Application.Clients;
using Application.Interfaces;
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
        private readonly IAiRiskAnalysisService _aiRisk;
        private readonly IRiskAlertService _riskAlert;

        public ClientAppService(
            DataContext context,
            ITaxService taxService,
            IOveruseService overuseService,
            IAiRiskAnalysisService aiRisk,
            IRiskAlertService riskAlert
        )
        {
            _context = context;
            _taxService = taxService;
            _overuseService = overuseService;
            _aiRisk = aiRisk;
            _riskAlert = riskAlert;
        }

        public async Task<ClientDashboardDto> GetDashboardDataAsync(
            Guid clientId,
            int year,
            int month,
            string currentUserName,
            bool isAdmin = false,
            bool isSenior = false
        )
        {
            var query = _context
                .Clients.Include(c => c.ClientTariffs)
                .Include(c => c.Transactions)
                    .ThenInclude(t => t.Service)
                .Include(c => c.Internal)
                .AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(c =>
                    c.Internal != null
                    && c.Internal.ResponsiblePersonContact.Contains(currentUserName)
                );
            }

            var client = await query.FirstOrDefaultAsync(c => c.Id == clientId);
            if (client == null)
                return null;

            var tariff = client.CurrentTariff;

            bool isAllTime = year <= 0 || month <= 0;
            int targetYear = isAllTime ? DateTime.Now.Year : year;
            int targetMonth = isAllTime ? DateTime.Now.Month : month;

            var stats = _taxService.GetTariffUsage(client, tariff, targetYear, targetMonth);

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

            // ------------------------------
            // BUILD DASHBOARD DTO
            // ------------------------------
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

                OverusedOperations = stats.OverusedOperations,
                OverusedMinutes = stats.OverusedMinutes,
                OverusedOperationsCost = stats.OverusedOperationsCost,
                OverusedMinutesCost = stats.OverusedMinutesCost,
            };

            // ------------------------------
            // LOCAL RISK SCORE (fallback)
            // ------------------------------
            dto.RiskScore = CalculateRiskScore(dto);

            var (level, color) = GetRiskCategory(dto.RiskScore);
            dto.RiskLevel = level;
            dto.RiskColor = color;

            dto.RiskRecommendations = BuildRiskRecommendations(dto);

            // ------------------------------
            // AI STRUCTURED RISK ANALYSIS
            // ------------------------------
            var aiRisk = await _aiRisk.AnalyzeRiskStructuredAsync(dto);

            if (aiRisk != null)
            {
                dto.RiskScore = aiRisk.RiskScore;
                dto.RiskLevel = aiRisk.RiskLevel;
                dto.RiskColor = aiRisk.RiskColor;
                dto.RiskRecommendations = aiRisk.Recommendations;
                dto.RiskInsight = aiRisk.Summary;
            }

            // ------------------------------
            // AI RISK FORECAST
            // ------------------------------
            dto.RiskForecast = await _aiRisk.ForecastRiskAsync(dto);

            // ------------------------------
            // AI GENERAL INSIGHT
            // ------------------------------
            dto.AiInsight = await _aiRisk.AnalyzeRiskAsync(dto);

            // ------------------------------
            // RISK ALERTS
            // ------------------------------
            await _riskAlert.CheckAndNotifyAsync(client, aiRisk);

            return dto;
        }

        // ------------------------------
        // LOCAL RISK CALCULATION (fallback)
        // ------------------------------
        private int CalculateRiskScore(ClientDashboardDto dto)
        {
            int score = 0;

            if (dto.EcpExpiryDate.HasValue)
            {
                var days = dto.DaysUntilEcpExpires;

                if (days <= 0)
                    score += 30;
                else if (days <= 7)
                    score += 25;
                else if (days <= 30)
                    score += 10;
            }

            score += dto.TaxRiskLevel switch
            {
                "High" => 30,
                "Medium" => 15,
                "Low" => 5,
                _ => 0,
            };

            if (dto.TotalOutstandingDebt > 1_000_000)
                score += 25;
            else if (dto.TotalOutstandingDebt > 300_000)
                score += 15;
            else if (dto.TotalOutstandingDebt > 50_000)
                score += 5;

            if (dto.OverusedOperations > 0)
                score += 10;
            if (dto.OverusedMinutes > 0)
                score += 10;

            if (dto.OverdueTasksCount > 0)
                score += Math.Min(dto.OverdueTasksCount * 3, 15);

            return Math.Min(score, 100);
        }

        private (string level, string color) GetRiskCategory(int score)
        {
            return score switch
            {
                <= 25 => ("Low", "Green"),
                <= 50 => ("Medium", "Yellow"),
                <= 75 => ("High", "Orange"),
                _ => ("Critical", "Red"),
            };
        }

        private string BuildRiskRecommendations(ClientDashboardDto dto)
        {
            var list = new List<string>();

            if (dto.DaysUntilEcpExpires <= 7)
                list.Add("Продлить ЭЦП в ближайшие 3–5 дней.");

            if (dto.TaxRiskLevel == "High")
                list.Add("Провести аудит налоговой отчётности.");

            if (dto.TotalOutstandingDebt > 300_000)
                list.Add("Связаться с клиентом для уточнения задолженности.");

            if (dto.OverdueTasksCount > 0)
                list.Add("Закрыть просроченные задачи.");

            if (dto.OverusedOperations > 0 || dto.OverusedMinutes > 0)
                list.Add("Пересмотреть тариф или обсудить перерасход.");

            return list.Count == 0 ? "Риски минимальны." : string.Join("\n", list);
        }
    }
}
