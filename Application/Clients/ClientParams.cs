using Application.Core;

namespace Application.Clients
{
    public class ClientParams : PagingParams
    {
        public string Name { get; set; }

        // Лист "Клиенты", колонка "ИИН/БИН"
        public string BinIin { get; set; }

        // Лист "Клиенты", колонка "Адрес"
        public string Address { get; set; }

        // Лист "Клиенты", колонка "Налоговый режим" (УР, ОУР и т.д.)
        public string TaxRegime { get; set; }

        // Лист "Клиенты", колонка "НДС" (Плательщик/Не плательщик)
        public string NdsStatus { get; set; }

        // Лист "Клиенты", колонка "Степень налогового риска"
        public string TaxRiskLevel { get; set; }
    }
}
