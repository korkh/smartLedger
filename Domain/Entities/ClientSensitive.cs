using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Common;

namespace Domain.Entities
{
    //Level 3 access
    public class ClientSensitive : BaseEntity
    {
        [Key]
        public Guid ClientId { get; set; }

        [ForeignKey(nameof(ClientId))]
        public virtual Client Client { get; set; }

        // Храним в зашифрованном виде (string)
        public string EcpPassword { get; set; }
        public string EsfPassword { get; set; }
        public string BankingPasswords { get; set; }

        public string StrategicNotes { get; set; }
        public string PersonalInfo { get; set; }
    }
}
