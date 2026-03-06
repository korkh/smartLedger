using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Client
    {
        [Key]
        public int Id { get; set; } // Matches int key in IdentityDbContext

        // --- LEVEL 1 (Basic Information) ---

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(12)] // Standard length for BIN/IIN in Kazakhstan
        public string BinIin { get; set; }

        public string Address { get; set; }

        public string TaxRegime { get; set; } // e.g., "УР", "ОУР"

        public string NdsStatus { get; set; } // Taxpayer or non-taxpayer

        public string TaxRiskLevel { get; set; } // Low, Medium, High risk

        // --- LEVEL 2 (Manager Access) ---

        public string ResponsiblePersonContact { get; set; }

        public string BankManagerContact { get; set; }

        public string ManagerNotes { get; set; }

        // --- LEVEL 3 (Admin / Confidential) ---

        public string EcpPassword { get; set; }

        public string EsfPassword { get; set; }

        public string BankingPasswords { get; set; }

        public string StrategicNotes { get; set; }

        public string PersonalInfo { get; set; }

        // Relationships
        public virtual ICollection<Transaction> Transactions { get; set; } =
            new List<Transaction>();

        // Navigation property for current tariff analysis
        public virtual ClientTariff CurrentTariff { get; set; }
    }
}
