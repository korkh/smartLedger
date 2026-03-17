using System.ComponentModel.DataAnnotations;
using Domain.Constants;
using Domain.Entities.Common;

namespace Domain.Entities
{
    public class Client : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string BinIin { get; set; }
        public string Address { get; set; }

        public TaxRegime TaxRegime { get; set; }

        public string NdsStatus { get; set; }
        public string TaxRiskLevel { get; set; }
        public string Oked { get; set; }

        public int EmployeesCount { get; set; }
        public DateTime? EcpExpiryDate { get; set; }

        public decimal TotalDebt { get; set; }

        // --- Тарифы клиента ---
        public ICollection<ClientTariff> ClientTariffs { get; set; } = new List<ClientTariff>();

        // --- Текущий тариф (не мапится в БД) ---
        public ClientTariff CurrentTariff =>
            ClientTariffs
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.ContractDate)
                .FirstOrDefault();

        // --- Транзакции клиента ---
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        // --- Уровни доступа ---
        public virtual ClientInternal Internal { get; set; }
        public virtual ClientSensitive Sensitive { get; set; }
    }
}
