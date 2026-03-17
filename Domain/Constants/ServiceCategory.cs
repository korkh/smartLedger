using System.ComponentModel.DataAnnotations;

namespace Domain.Constants
{
    public enum ServiceCategory
    {
        [Display(Name = "Не задано")]
        None = 0,

        [Display(Name = "СНТ / ЭАВР")]
        Snt = 1,

        [Display(Name = "Приём на работу")]
        Hiring = 2,

        [Display(Name = "Счет на оплату")]
        Invoice = 3,

        [Display(Name = "Номенклатура")]
        Nomenclature = 4,

        [Display(Name = "Контрагент")]
        Counterparty = 5,

        [Display(Name = "Доверенность")]
        PowerOfAttorney = 6,

        [Display(Name = "Реализация ТМЗ")]
        Sales = 7,

        [Display(Name = "ЭСФ")]
        Esf = 8,

        [Display(Name = "Платежное поручение")]
        PaymentOrder = 9,

        [Display(Name = "Разноска выписки банка")]
        BankStatement = 10,

        [Display(Name = "Коммуникации с клиентом")]
        Communication = 11,

        [Display(Name = "Принятие к учету ОС/НМА")]
        AssetRecognition = 12,

        [Display(Name = "Авансовый отчет")]
        AdvanceReport = 13,

        [Display(Name = "Акт сверки")]
        ReconciliationAct = 14,

        [Display(Name = "Возврат ТМЗ")]
        InventoryReturn = 15,

        [Display(Name = "ГТД по импорту")]
        ImportDeclaration = 16,

        [Display(Name = "Командировки")]
        BusinessTrip = 17,

        [Display(Name = "Кадровое перемещение")]
        HRMovement = 18,

        [Display(Name = "Корректировка долга")]
        DebtAdjustment = 19,

        [Display(Name = "Поступление ТМЗ")]
        InventoryReceipt = 20,

        [Display(Name = "Исходящая корреспонденция")]
        OutgoingMail = 21,

        [Display(Name = "ПКО / РКО")]
        CashOrder = 22,

        [Display(Name = "Фискальный чек")]
        FiscalReceipt = 23,

        [Display(Name = "Списание ТМЗ")]
        InventoryWriteOff = 24,

        [Display(Name = "Увольнение")]
        Dismissal = 25,

        [Display(Name = "ЭДВС")]
        EDVS = 26,

        [Display(Name = "Перемещение ТМЗ")]
        InventoryTransfer = 27,

        [Display(Name = "Комплектация ТМЗ")]
        InventoryAssembly = 28,

        [Display(Name = "Статистический отчет")]
        StatisticalReport = 29,

        [Display(Name = "Налоговый отчет")]
        TaxReport = 30,

        [Display(Name = "Регистрация прочих доходов")]
        OtherIncome = 31,

        [Display(Name = "Поиск / исправление источника")]
        SourceFix = 32,

        [Display(Name = "Восстановление учета")]
        AccountingRecovery = 33,

        [Display(Name = "Постановка / снятие с учета ККМ")]
        KkmRegistration = 34,

        [Display(Name = "Регистрация валютного контракта")]
        CurrencyContract = 35,

        [Display(Name = "Проверка расчетного счета")]
        BankAccountCheck = 36,

        [Display(Name = "Договор")]
        Contract = 37,

        [Display(Name = "Валютные операции")]
        CurrencyOperations = 38,

        [Display(Name = "Проверка кабинета налогоплательщика")]
        TaxpayerCabinet = 39,

        [Display(Name = "Бухгалтерская справка")]
        AccountingNote = 40,

        [Display(Name = "Финансовая отчетность")]
        FinancialReport = 41,

        [Display(Name = "Закрытие месяца")]
        MonthClosing = 42,
    }
}
