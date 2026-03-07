using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Represents a work entry from the "TRANSACTIONS" sheet.
    /// </summary>
    public class Transaction
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        // Foreign key to Client
        public Guid ClientId { get; set; }

        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }

        // Foreign key to Service Reference (from Directory sheet)
        public Guid ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public virtual ServiceReference Service { get; set; }

        public string PerformerName { get; set; } // Performed by (e.g., Dinara M)

        public int OperationsCount { get; set; }

        public int ActualTimeMinutes { get; set; }

        public int BillableTimeMinutes { get; set; }

        public int CommunicationTimeMinutes { get; set; }

        public string Status { get; set; } // e.g., "In Progress", "Completed"

        [Column(TypeName = "decimal(18,2)")]
        public decimal ExtraServiceAmount { get; set; }
    }
}
