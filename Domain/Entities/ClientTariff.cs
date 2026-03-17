using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Constants;
using Domain.Entities.Common;

namespace Domain.Entities
{
    public class ClientTariff : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ClientId { get; set; }

        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }
        public TariffPackage Package { get; set; }

        // --- Финансовые параметры ---
        public decimal MonthlyFee { get; set; } // СУММА ДОГОВОРА
        public decimal TailAmount { get; set; } // Сумма по хвостам (Excel)
        public decimal Price => MonthlyFee;

        // --- Лимиты операций ---
        public int OperationsLimit { get; set; } // План операций
        public int CarriedOverOperations { get; set; } // Перенос операций
        public int TotalOperationsLimit => OperationsLimit + CarriedOverOperations;

        // --- Лимиты по времени (коммуникации) ---
        public int CommunicationMinutesLimit { get; set; } // План минут
        public int CarriedOverMinutes { get; set; } // Перенос минут
        public int TotalMinutesLimit => CommunicationMinutesLimit + CarriedOverMinutes;

        // --- Лимиты по отчётам ---
        public int StatisticalReportsLimit { get; set; }
        public int MonthlyTaxReportsLimit { get; set; }
        public int QuarterlyTaxReportsLimit { get; set; }
        public int SemiAnnualTaxReportsLimit { get; set; }
        public int AnnualTaxReportsLimit { get; set; }

        // --- Кадровый учёт ---
        public int EmployeeCountLimit { get; set; } // *кол-во сотрудников

        // --- Включённые услуги (флаги) ---
        public bool IncludesHR { get; set; } // Кадровый учет
        public bool IncludesMonthlyReports { get; set; }
        public bool IncludesQuarterlyReports { get; set; }
        public bool IncludesSemiAnnualReports { get; set; }
        public bool IncludesAnnualReports { get; set; }

        // --- Даты ---
        public DateTime ContractDate { get; set; } // Дата начала
        public DateTime? ContractSigningDate { get; set; } // Дата подписания договора

        // --- Статус ---
        public bool IsActive { get; set; } = true;
    }
}
