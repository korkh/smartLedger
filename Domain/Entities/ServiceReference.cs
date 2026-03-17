using System.ComponentModel.DataAnnotations;
using Domain.Constants;
using Domain.Entities.Common;

namespace Domain.Entities
{
    public class ServiceReference : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public string Description { get; set; }

        // Месяц из Excel (1–12)
        public int MonthNumber { get; set; }

        // Плановые показатели
        public int PlannedOperationsPerMonth { get; set; }
        public int PlannedMinutesPerMonth { get; set; }

        // Новый enum
        public ServiceStatus Status { get; set; }

        // Стандартное время выполнения
        public int StandardTimeMinutes { get; set; }

        // Стоимость разовой услуги
        public decimal BasePrice { get; set; }

        // Ответственный сотрудник
        public string DefaultPerformerName { get; set; }

        // Категория услуги (новый enum)
        public ServiceCategory Category { get; set; }

        // Налоговые режимы (Excel позволяет несколько)
        // Храним как CSV: "OUR,USN,Patent"
        public string ApplicableTaxRegimes { get; set; }

        // Влияет ли на порог НДС
        public bool AffectsNdsThreshold { get; set; }

        // Разовая услуга
        public bool IsExtraService { get; set; }

        // Фото / иконка
        public string IconUrl { get; set; }

        // Связь с транзакциями
        public virtual ICollection<Transaction> Transactions { get; set; } =
            new List<Transaction>();
    }
}
