using Application.Transactions;

namespace Application.Clients
{
    public class ClientDto
    {
        // --- УРОВЕНЬ 1 (Базовая информация) ---

        // Лист "Клиенты", колонка "Наименование"
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

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

        // --- УРОВЕНЬ 2 (Менеджерский доступ) ---

        // Лист "Клиенты", колонка "Контакты ответственного лица клиента"
        public string ResponsiblePersonContact { get; set; }

        // Лист "Клиенты" (2-й уровень), колонка "Номер менеджера банка"
        public string BankManagerContact { get; set; }

        // Лист "Клиенты" (2-й уровень), колонка "Заметки 2 ур."
        public string ManagerNotes { get; set; }

        // --- УРОВЕНЬ 3 (Административный / Конфиденциальный) ---

        // Лист "Клиенты", колонка "Пароли ЭЦП"
        public string EcpPassword { get; set; }

        // Лист "Клиенты", колонка "Пароли ИСС ЭСФ"
        public string EsfPassword { get; set; }

        // Лист "Клиенты", колонка "Пароли от банкинга"
        public string BankingPasswords { get; set; }

        // Лист "Клиенты" (3-й уровень), колонка "Стратегические заметки по развитию"
        public string StrategicNotes { get; set; }

        // Лист "Клиенты" (3-й уровень), колонка "Личная информация по клиенту"
        public string PersonalInfo { get; set; }

        // Связи
        public ICollection<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
        public ClientTariffDto CurrentTariff { get; set; }
    }
}
