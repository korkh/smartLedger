using Application.Core;

namespace Application.Clients
{
    public class ClientParams : PagingParams
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BinIin { get; set; }
        public string Address { get; set; }
        public string TaxRegime { get; set; }
        public string NdsStatus { get; set; }
        public string TaxRiskLevel { get; set; }

        // --- Новые поля для фильтрации ЭЦП ---

        // Если true — показываем только тех, у кого скоро истекает срок
        public bool EcpWarningOnly { get; set; } = false;

        // Порог предупреждения в днях (стандартно 14)
        public int EcpWarningDays { get; set; } = 14;

        public string SortField { get; set; } = "firstname";
        public string Order { get; set; } = "asc";
        public string Search { get; set; }
        public string ResponsiblePersonContact { get; set; }
        public string TaskRiskLevel { get; set; }
        public string Oked { get; set; }
    }
}
