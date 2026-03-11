namespace Application.Transactions
{
    public class TransactionDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } // Just name for UI
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } // Just name for UI
        public string PerformerName { get; set; }
        public int OperationsCount { get; set; }
        public int ActualTimeMinutes { get; set; }
        public int BillableTimeMinutes { get; set; }
        public int CommunicationTimeMinutes { get; set; }
        public string Status { get; set; }
        public decimal ExtraServiceAmount { get; set; }
        public int StatReports { get; set; }
        public int MonthlyTaxReports { get; set; }
        public int QuarterlyTaxReports { get; set; }
        public int SemiAnnualTaxReports { get; set; }
        public int AnnualTaxReports { get; set; }
    }
}
