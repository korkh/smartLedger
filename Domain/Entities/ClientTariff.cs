using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class ClientTariff
    {
        [Key]
        public int Id { get; set; }

        public int ClientId { get; set; }

        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }

        // Monthly contract amount (СУММА ДОГОВОРА)
        public decimal MonthlyFee { get; set; }

        // Maximum allowed operations in the current month
        public int OperationsLimit { get; set; }

        // Maximum communication/consultation time allowed in minutes
        public int CommunicationMinutesLimit { get; set; }

        // Operations carried over from the previous month (Перенесенные операции)
        public int CarriedOverOperations { get; set; }

        // Minutes carried over from the previous month (Перенесенное время)
        public int CarriedOverMinutes { get; set; }

        public DateTime ContractDate { get; set; }

        // Useful for tracking historical changes of tariffs
        public bool IsActive { get; set; } = true;
    }
}
