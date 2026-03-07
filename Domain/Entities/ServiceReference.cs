using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class ServiceReference
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        // Standard time to complete the service in minutes
        public int StandardTimeMinutes { get; set; }

        // Base cost of the service for one-off tasks
        public decimal BasePrice { get; set; }

        // Default employee assigned to this service (e.g., "Nagima", "Dinara M")
        public string DefaultPerformerName { get; set; }

        // Indicates if this service counts towards the VAT registration threshold (realization/income)
        public bool AffectsNdsThreshold { get; set; }

        // Relationship: one service can be linked to many transaction entries
        public virtual ICollection<Transaction> Transactions { get; set; } =
            new List<Transaction>();
    }
}
