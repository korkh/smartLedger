using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Common;

namespace Domain.Entities
{
    public class ClientTariff : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ClientId { get; set; }

        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }

        // Monthly contract amount (СУММА ДОГОВОРА)
        public decimal MonthlyFee { get; set; }
        public decimal Price => MonthlyFee; // Алиас для совместимости с сервисом

        // Maximum allowed operations in the current month
        public int OperationsLimit { get; set; }

        // Maximum communication/consultation time allowed in minutes
        public int CommunicationMinutesLimit { get; set; }
        public int ConsultingLimit => CommunicationMinutesLimit;

        // Operations carried over from the previous month (Перенесенные операции)
        public int CarriedOverOperations { get; set; }

        // Minutes carried over from the previous month (Перенесенное время)
        public int CarriedOverMinutes { get; set; }

        public DateTime ContractDate { get; set; }

        // Useful for tracking historical changes of tariffs
        public bool IsActive { get; set; } = true;
    }
}
