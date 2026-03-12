using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Constants;
using Domain.Entities.Common;

namespace Domain.Entities
{
    public class Transaction : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public Guid ClientId { get; set; }

        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }

        //Service
        public ServiceType ServiceType { get; set; }
        public Guid? ServiceId { get; set; }
        public virtual ServiceReference Service { get; set; }

        public string PerformerName { get; set; }
        public int OperationsCount { get; set; }

        public int ActualTimeMinutes { get; set; }
        public int BillableTimeMinutes { get; set; }
        public int CommunicationTimeMinutes { get; set; }

        public string Status { get; set; }
        public bool IsExtraService { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ExtraServiceAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NdsBaseAmount { get; set; }
    }
}
