// Domain/Entities/Invoice.cs
using Domain.Entities.Common;

namespace Domain.Entities
{
    public class Invoice : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid ClientId { get; set; }
        public Client Client { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }

        public decimal TariffAmount { get; set; }
        public decimal ExtraServicesAmount { get; set; }
        public decimal OveruseAmount { get; set; }

        public decimal TotalAmount => TariffAmount + ExtraServicesAmount + OveruseAmount;

        public bool IsPaid { get; set; }
    }
}
