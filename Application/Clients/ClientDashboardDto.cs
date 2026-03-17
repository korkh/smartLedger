using Application.Interfaces;

namespace Application.Clients
{
    public class ClientDashboardDto
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string TaxRegime { get; set; }
        public string TaxRiskLevel { get; set; }

        public int PersonnelCount { get; set; }

        // --- OPERATIONS ---
        public int OperationsLimit { get; set; }
        public int OperationsActual { get; set; }
        public int OperationsRemaining => OperationsLimit - OperationsActual;

        public int ConsultingMinutesLimit { get; set; }
        public int ConsultingMinutesActual { get; set; }

        // --- REPORTS ---
        public int StatReportsCount { get; set; }
        public int MonthlyTaxReports { get; set; }
        public int QuarterlyTaxReports { get; set; }
        public int SemiAnnualTaxReports { get; set; }
        public int AnnualTaxReports { get; set; }

        // --- FINANCES ---
        public decimal TariffAmount { get; set; }
        public decimal MonthlyExtraServicesAmount { get; set; }
        public decimal TotalOutstandingDebt { get; set; }

        public decimal TotalToPay => TariffAmount + MonthlyExtraServicesAmount + TotalOveruseCost;

        // --- VAT ---
        public decimal NdsThreshold { get; set; }
        public decimal MonthlyTurnover { get; set; }
        public decimal CurrentYearTurnover { get; set; }

        public double NdsProgressPercentage =>
            NdsThreshold <= 0
                ? 0
                : Math.Round((double)(CurrentYearTurnover / NdsThreshold) * 100, 2);

        // --- DEADLINES ---
        public DateTime? EcpExpiryDate { get; set; }
        public int DaysUntilEcpExpires { get; set; }

        public int ActiveTasksCount { get; set; }
        public int OverdueTasksCount { get; set; }

        // --- OVERUSE (ХВОСТЫ) ---
        public int OverusedOperations { get; set; }
        public int OverusedMinutes { get; set; }

        public decimal OverusedOperationsCost { get; set; }
        public decimal OverusedMinutesCost { get; set; }

        public decimal TotalOveruseCost => OverusedOperationsCost + OverusedMinutesCost;

        public string AiInsight { get; set; }

        // --- AI RISK ANALYSIS ---
        public string RiskInsight { get; set; }

        // --- STRUCTURED RISK METRICS ---
        public int RiskScore { get; set; } // 0–100
        public string RiskLevel { get; set; } // Low / Medium / High / Critical
        public string RiskColor { get; set; } // Green / Yellow / Orange / Red
        public string RiskRecommendations { get; set; }

        public RiskForecastResult RiskForecast { get; set; }
    }
}
