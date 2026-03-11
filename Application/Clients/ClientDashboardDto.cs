public class ClientDashboardDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string TaxRegime { get; set; }
    public string TaxRiskLevel { get; set; }

    // --- БЛОК 1: ОПЕРАЦИИ (Лист "Dash board" - План/Факт) ---
    public int OperationsLimit { get; set; }
    public int OperationsActual { get; set; }
    public int OperationsRemaining => OperationsLimit - OperationsActual;

    public int ConsultingMinutesLimit { get; set; }
    public int ConsultingMinutesActual { get; set; }

    // --- БЛОК 2: ОТЧЕТНОСТЬ (Колонки из "ТЕКУЩИЙ ТАРИФ КЛИЕНТА") ---
    public int StatReportsCount { get; set; }
    public int MonthlyTaxReports { get; set; }
    public int QuarterlyTaxReports { get; set; }
    public int SemiAnnualTaxReports { get; set; }
    public int AnnualTaxReports { get; set; }
    public int PersonnelCount { get; set; } // Кадровый учет

    // --- БЛОК 3: ФИНАНСЫ (Обновлено) ---
    public decimal TariffAmount { get; set; }

    // Хвосты именно за ВЫБРАННЫЙ месяц (из транзакций)
    public decimal MonthlyExtraServicesAmount { get; set; }

    // Общая задолженность клиента (из сущности Client)
    public decimal TotalOutstandingDebt { get; set; }

    // Итого к оплате за месяц (Тариф + новые хвосты)
    public decimal TotalInvoicedThisMonth => TariffAmount + MonthlyExtraServicesAmount;

    // Общая сумма, которую мы ждем от клиента прямо сейчас
    public decimal GrandTotalDue => TotalOutstandingDebt;

    // --- БЛОК 4: ЛИМИТЫ НДС И РЕЖИМА (Лист "Справочник") ---
    public decimal NdsThreshold { get; set; } // Предел 30 000 МРП
    public decimal CurrentYearTurnover { get; set; }
    public double NdsProgressPercentage =>
        NdsThreshold > 0 ? (double)(CurrentYearTurnover / NdsThreshold) * 100 : 0;

    // --- БЛОК 5: СРОКИ И ЗАДАЧИ ---
    public DateTime? EcpExpiryDate { get; set; }
    public int DaysUntilEcpExpires { get; set; }
    public int ActiveTasksCount { get; set; }
    public int OverdueTasksCount { get; set; }

    public string AiInsight { get; set; }
}
