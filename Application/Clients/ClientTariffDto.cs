namespace Application.Clients
{
    public class ClientTariffDto
    {
        // --- Финансы ---
        public decimal ContractAmount { get; set; }
        public decimal TailAmount { get; set; }

        // --- Даты ---
        public DateTime StartDate { get; set; }
        public DateTime? ContractSigningDate { get; set; }

        // --- Операции ---
        public int AllowedOperations { get; set; }
        public int CarriedOverOperations { get; set; }
        public int TotalOperationsLimit => AllowedOperations + CarriedOverOperations;

        // --- Коммуникации ---
        public int AllowedCommunicationMinutes { get; set; }
        public int CarriedOverMinutes { get; set; }
        public int TotalMinutesLimit => AllowedCommunicationMinutes + CarriedOverMinutes;

        // --- Лимиты по отчётам ---
        public int StatisticalReportsLimit { get; set; }
        public int MonthlyTaxReportsLimit { get; set; }
        public int QuarterlyTaxReportsLimit { get; set; }
        public int SemiAnnualTaxReportsLimit { get; set; }
        public int AnnualTaxReportsLimit { get; set; }

        // --- Кадры ---
        public int EmployeeCountLimit { get; set; }

        // --- Включённые услуги ---
        public bool IncludesHR { get; set; }
        public bool IncludesMonthlyReports { get; set; }
        public bool IncludesQuarterlyReports { get; set; }
        public bool IncludesSemiAnnualReports { get; set; }
        public bool IncludesAnnualReports { get; set; }

        // -- Перерасход ---
        public int OverusedOperations { get; set; }
        public int OverusedMinutes { get; set; }

        public decimal OverusedOperationsCost { get; set; }
        public decimal OverusedMinutesCost { get; set; }

        public decimal TotalOveruseCost => OverusedOperationsCost + OverusedMinutesCost;
    }
}
