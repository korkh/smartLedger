namespace Application.Clients
{
    public class ClientLevel1Dto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BinIin { get; set; }
        public string Address { get; set; }

        // Enum → string
        public string TaxRegime { get; set; }

        public string NdsStatus { get; set; }
        public string TaxRiskLevel { get; set; }
        public string Oked { get; set; }
        public int EmployeesCount { get; set; }
        public DateTime? EcpExpiryDate { get; set; }
        public decimal TotalDebt { get; set; }
    }
}
