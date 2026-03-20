namespace Application.Clients
{
    public class ClientDto
    {
        public Guid Id { get; set; }

        // LEVEL 1
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BinIin { get; set; }
        public string Address { get; set; }
        public string TaxRegime { get; set; }
        public string NdsStatus { get; set; }
        public string TaxRiskLevel { get; set; }
        public string Oked { get; set; }
        public int EmployeesCount { get; set; }
        public DateTime? EcpExpiryDate { get; set; }
        public decimal TotalDebt { get; set; }

        // FRONTEND-ONLY
        public int? DaysUntilEcpExpires { get; set; }

        // LEVEL 2
        public string ResponsiblePersonContact { get; set; }
        public string BankManagerContact { get; set; }
        public string ManagerNotes { get; set; }

        // LEVEL 3
        public string EcpPassword { get; set; }
        public string EsfPassword { get; set; }
        public string BankingPasswords { get; set; }
        public string StrategicNotes { get; set; }
        public string PersonalInfo { get; set; }
    }
}
