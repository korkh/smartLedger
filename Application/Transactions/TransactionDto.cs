using Domain.Constants;

namespace Application.Transactions
{
    public class TransactionDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }

        // Связи
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } // Для отображения в списке

        // Тип услуги (Enum — наш единственный источник правды)
        public ServiceType ServiceType { get; set; }
        public string ServiceTypeName { get; set; } // Локализованное имя из [Display]

        public string PerformerName { get; set; }

        // Количественные показатели конкретной транзакции
        public int OperationsCount { get; set; }
        public int ActualTimeMinutes { get; set; }
        public int BillableTimeMinutes { get; set; }
        public int CommunicationTimeMinutes { get; set; }

        public string Status { get; set; }

        // Финансовые показатели
        public bool IsExtraService { get; set; }
        public decimal ExtraServiceAmount { get; set; }

        // База для НДС (если применимо к этому типу услуги)
        public decimal NdsBaseAmount { get; set; }
    }
}
