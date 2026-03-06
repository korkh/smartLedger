namespace Application.Clients
{
    public class ClientDashboardDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TaxRegime { get; set; }

        // --- НДС ---
        public decimal RemainingNdsLimit { get; set; }
        public double NdsUsagePercentage { get; set; }

        // --- ЛИМИТ ОПЕРАЦИЙ ---
        public int RemainingOperations { get; set; }
        public double OperationsUsagePercentage { get; set; }

        // --- ЛИМИТ КОНСУЛЬТАЦИЙ (МИНУТЫ) ---
        public int RemainingCommunicationMinutes { get; set; }
        public double CommunicationUsagePercentage { get; set; }
    }
}
