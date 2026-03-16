using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Common;

namespace Domain.Entities
{
    //Level 2 access
    public class ClientInternal : BaseEntity
    {
        [Key]
        public Guid ClientId { get; set; }

        [ForeignKey(nameof(ClientId))]
        public Client Client { get; set; }

        public string ResponsiblePersonContact { get; set; }
        public string BankManagerContact { get; set; }
        public string ManagerNotes { get; set; }
    }
}
