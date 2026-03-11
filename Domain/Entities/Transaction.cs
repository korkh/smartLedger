using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Common;

namespace Domain.Entities
{
    public class Transaction : BaseEntity
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
        public int ConsultingMinutes => CommunicationTimeMinutes;

        public int? StatReports { get; set; }
        public int? MonthlyTaxReports { get; set; }
        public int? QuarterlyTaxReports { get; set; }
        public int? SemiAnnualTaxReports { get; set; }
        public int? AnnualTaxReports { get; set; }

        public string Status { get; set; } // e.g., "In Progress", "Completed"

        public string ServiceType { get; set; } // "Stat", "TaxMonthly" и т.д.
        public bool IsExtraService { get; set; } // Флаг "хвостов" (доп. услуг)

        [Column(TypeName = "decimal(18,2)")]
        public decimal ExtraServiceAmount { get; set; }

        // Сумма для расчетов (если это доп. услуга)
        [Column(TypeName = "decimal(18,2)")]
        public decimal NdsBaseAmount { get; set; }
    }
}
