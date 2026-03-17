// Domain/Entities/TariffHistory.cs
using Domain.Entities.Common;

namespace Domain.Entities
{
    public class TariffHistory : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid ClientId { get; set; }
        public Client Client { get; set; }

        public Guid TariffId { get; set; }
        public ClientTariff Tariff { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }

        public int UsedOperations { get; set; }
        public int UsedMinutes { get; set; }

        public int OverusedOperations { get; set; }
        public int OverusedMinutes { get; set; }

        public decimal OverusedOperationsCost { get; set; }
        public decimal OverusedMinutesCost { get; set; }

        public decimal TariffAmount { get; set; }
        public decimal ExtraServicesAmount { get; set; }
        public decimal TotalToPay { get; set; }

        public int RiskScore { get; set; }
        public string RiskLevel { get; set; }
        public string RiskColor { get; set; }
        public string RiskRecommendations { get; set; }
    }
}
