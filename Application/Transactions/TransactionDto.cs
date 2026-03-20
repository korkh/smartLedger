using Domain.Constants;

namespace Application.Transactions
{
    public class TransactionDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }

        public Guid ClientId { get; set; }
        public string ClientName { get; set; }

        public ServiceCategory ServiceCategory { get; set; }
        public string ServiceCategoryName { get; set; }
        public string PerformerName { get; set; }

        public int OperationsCount { get; set; }

        public int ActualTimeMinutes { get; set; }
        public int BillableTimeMinutes { get; set; }
        public int CommunicationTimeMinutes { get; set; }
        public int TeamWorkMinutes { get; set; }
        public int ForceMajeureMinutes { get; set; }
        public int TotalDeductibleMinutes { get; set; }

        public string Status { get; set; }

        public bool IsExtraService { get; set; }
        public decimal ExtraServiceAmount { get; set; }
        public int ExtraServiceCount { get; set; }

        public decimal NdsBaseAmount { get; set; }

        public bool IsMonthlyReport { get; set; }
        public bool IsQuarterlyReport { get; set; }
        public bool IsSemiAnnualReport { get; set; }
        public bool IsAnnualReport { get; set; }
        public bool IsStatReport { get; set; }
        public bool IsReport5 { get; set; }

        public string OperationNote { get; set; }
        public string ServiceNote { get; set; }
    }
}
