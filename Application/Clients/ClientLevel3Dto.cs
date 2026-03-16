namespace Application.Clients
{
    public class ClientLevel3Dto : ClientLevel2Dto
    {
        public string EcpPassword { get; set; }
        public string EsfPassword { get; set; }
        public string BankingPasswords { get; set; }
        public string StrategicNotes { get; set; }
        public string PersonalInfo { get; set; }
    }
}
