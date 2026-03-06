namespace Application.Clients
{
    public class ClientTariffDto
    {
        // Базовая информация о договоре
        public decimal ContractAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? ContractSigningDate { get; set; }

        // Лимиты операций (то, что идет в "План" на спидометрах)
        public int AllowedOperations { get; set; }
        public int CarriedOverOperations { get; set; }
        public int TotalOperationsLimit => AllowedOperations + CarriedOverOperations;

        // Лимиты по времени (консультации)
        public int AllowedCommunicationMinutes { get; set; }
        public int CarriedOverMinutes { get; set; }
        public int TotalMinutesLimit => AllowedCommunicationMinutes + CarriedOverMinutes;

        // Налоговая отчетность (количественные лимиты)
        public int StatisticalReportsLimit { get; set; }
        public int MonthlyTaxReportsLimit { get; set; }
        public int QuarterlyTaxReportsLimit { get; set; }
        public int SemiAnnualTaxReportsLimit { get; set; }
        public int AnnualTaxReportsLimit { get; set; }

        // Кадровый учет
        public int EmployeeCountLimit { get; set; }
    }
}
