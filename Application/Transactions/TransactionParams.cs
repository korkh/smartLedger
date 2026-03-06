using Application.Core;

namespace Application.Transactions
{
    public class TransactionParams : PagingParams
    {
        public string Search { get; set; }
        public Guid? ClientId { get; set; }
        public Guid? ServiceId { get; set; }
        public string Status { get; set; }

        // Фильтрация по периоду для отчетов
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Сортировка
        public string SortField { get; set; } = "Date";
        public string Order { get; set; } = "desc";
    }
}
