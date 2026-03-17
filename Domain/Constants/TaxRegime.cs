using System.ComponentModel.DataAnnotations;

namespace Domain.Constants
{
    public enum TaxRegime
    {
        [Display(Name = "Не задано")]
        None = 0,

        [Display(Name = "ОУР")]
        OUR = 1,

        [Display(Name = "УСН")]
        USN = 2,

        [Display(Name = "Патент")]
        Patent = 3,

        [Display(Name = "Фиксированный вычет")]
        FixedDeduction = 4,

        [Display(Name = "Розничный налог")]
        RetailTax = 5,
    }
}
