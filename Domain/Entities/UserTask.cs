using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Common;

namespace Domain.Entities
{
    public class UserTask : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime Deadline { get; set; }
        public bool IsCompleted { get; set; }

        public Guid ClientId { get; set; }

        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }
    }
}
