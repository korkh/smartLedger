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

        // Категория услуги (новый enum)
        public ServiceCategory Category { get; set; }

        public Guid? ServiceId { get; set; }
        public virtual ServiceReference Service { get; set; }

        public string PerformerName { get; set; }

        // Количественные показатели
        public int OperationsCount { get; set; }

        // Время
        public int ActualTimeMinutes { get; set; }
        public int BillableTimeMinutes { get; set; }
        public int CommunicationTimeMinutes { get; set; }
        public int TeamWorkMinutes { get; set; }
        public int ForceMajeureMinutes { get; set; }

        public int TotalDeductibleMinutes =>
            BillableTimeMinutes + CommunicationTimeMinutes + TeamWorkMinutes + ForceMajeureMinutes;

        public string Status { get; set; }

        // Разовые услуги
        public bool IsExtraService { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ExtraServiceAmount { get; set; }

        public int ExtraServiceCount { get; set; }

        // НДС
        [Column(TypeName = "decimal(18,2)")]
        public decimal NdsBaseAmount { get; set; }

        // Отчётность
        public bool IsMonthlyReport { get; set; }
        public bool IsQuarterlyReport { get; set; }
        public bool IsSemiAnnualReport { get; set; }
        public bool IsAnnualReport { get; set; }
        public bool IsStatReport { get; set; }
        public bool IsReport5 { get; set; }

        // Примечания
        public string OperationNote { get; set; }
        public string ServiceNote { get; set; }
    }
}
