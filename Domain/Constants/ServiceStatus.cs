using System.ComponentModel.DataAnnotations;

namespace Domain.Constants
{
    public enum ServiceStatus
    {
        [Display(Name = "Не задано")]
        None = 0,

        [Display(Name = "В процессе")]
        InProgress = 1,

        [Display(Name = "Завершено")]
        Completed = 2,

        [Display(Name = "На доработке")]
        NeedsRevision = 3,

        [Display(Name = "К выполнению")]
        Pending = 4,
    }
}
