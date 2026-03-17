namespace Domain.Entities
{
    public class TariffUsageStats
    {
        public int RemainingOperations { get; set; }
        public double OperationsPercentage { get; set; }

        public int RemainingMinutes { get; set; }
        public double MinutesPercentage { get; set; }

        // --- Хвосты ---
        public int OverusedOperations { get; set; }
        public int OverusedMinutes { get; set; }

        public decimal OverusedOperationsCost { get; set; }
        public decimal OverusedMinutesCost { get; set; }

        // --- Отчётность ---
        public int StatReportsCount { get; set; }
        public int MonthlyTaxReports { get; set; }
        public int QuarterlyTaxReports { get; set; }
        public int SemiAnnualTaxReports { get; set; }
        public int AnnualTaxReports { get; set; }
    }
}
