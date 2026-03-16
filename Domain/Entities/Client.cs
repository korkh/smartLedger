using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Common;

namespace Domain.Entities
{
    public class Client : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        // LEVEL 1 — публичные данные
        [Required, MaxLength(255)]
        public string FirstName { get; set; }

        [Required, MaxLength(255)]
        public string LastName { get; set; }

        [MaxLength(12)]
        public string BinIin { get; set; }

        public string Address { get; set; }
        public string TaxRegime { get; set; }
        public string NdsStatus { get; set; }
        public string TaxRiskLevel { get; set; }
        public string Oked { get; set; }
        public int EmployeesCount { get; set; }
        public DateTime? EcpExpiryDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDebt { get; set; }

        // Навигация
        //Level 2 access
        public virtual ClientInternal Internal { get; set; }
        public virtual ClientSensitive Sensitive { get; set; }
        public virtual ClientTariff CurrentTariff { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; } = [];
    }
}
