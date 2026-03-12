using System.ComponentModel.DataAnnotations;

namespace Domain.Constants
{
    public enum ServiceType
    {
        None = 0,

        [Display(Name = "Разноска выписки банка")]
        BankStatement = 1,

        [Display(Name = "Расчет налогов")]
        TaxCalculation = 2,

        [Display(Name = "ЭАВР / СНТ")]
        CargoCustoms = 3,

        [Display(Name = "Списание ТМЗ")]
        InventoryWriteOff = 4,

        [Display(Name = "Прием на работу / ЗП")]
        Payroll = 5,

        [Display(Name = "Стат. отчет")]
        StatReport = 6,

        [Display(Name = "Месячный налоговый отчет")]
        TaxReport = 7,

        [Display(Name = "Квартальный налоговый отчет")]
        QuarterlyTaxReport = 8,

        [Display(Name = "Полугодовой налоговый отчет")]
        SemiAnnualTaxReport = 9,

        [Display(Name = "Годовой налоговый отчет")]
        AnnualTaxReport = 10,
    }
}
