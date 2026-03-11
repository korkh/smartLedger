namespace Application.Clients
{
    public class ClientDashboardDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TaxRegime { get; set; }
        public string TaxRiskLevel { get; set; }

        // --- БЛОК 1: ОПЕРАЦИИ (Лимит vs Факт) ---
        public int OperationsLimit { get; set; }
        public int OperationsActual { get; set; }
        public int OperationsRemaining => OperationsLimit - OperationsActual;

        public int ConsultingMinutesLimit { get; set; }
        public int ConsultingMinutesActual { get; set; }

        // --- БЛОК 2: ОТЧЕТНОСТЬ ---
        public int StatReportsCount { get; set; }
        public int MonthlyTaxReports { get; set; }
        public int QuarterlyTaxReports { get; set; }
        public int SemiAnnualTaxReports { get; set; }
        public int AnnualTaxReports { get; set; }
        public int PersonnelCount { get; set; }

        // --- БЛОК 3: ФИНАНСЫ ---
        public decimal TariffAmount { get; set; }

        // "Хвосты" - сумма ExtraServiceAmount из транзакций за выбранный месяц
        public decimal MonthlyExtraServicesAmount { get; set; }

        // Общая задолженность из Client.TotalDebt
        public decimal TotalOutstandingDebt { get; set; }

        // К оплате за конкретный месяц
        public decimal TotalToPay => TariffAmount + MonthlyExtraServicesAmount;

        // Общий итог к получению
        public decimal GrandTotalDue => TotalOutstandingDebt;

        // --- БЛОК 4: ЛИМИТЫ НДС (На основе NdsBaseAmount из транзакций) ---

        // Порог (например, 20 000 или 30 000 МРП из справочника)
        public decimal NdsThreshold { get; set; }

        // Оборот только за текущий (выбранный) месяц
        public decimal MonthlyTurnover { get; set; }

        // Сумма NdsBaseAmount всех транзакций клиента за текущий календарный год
        public decimal CurrentYearTurnover { get; set; }

        // Процент приближения к порогу НДС
        public double NdsProgressPercentage
        {
            get
            {
                if (NdsThreshold <= 0)
                    return 0;
                var percentage = (double)(CurrentYearTurnover / NdsThreshold) * 100;
                return Math.Round(percentage, 2);
            }
        }

        // --- БЛОК 5: СРОКИ И ЗАДАЧИ ---
        public DateTime? EcpExpiryDate { get; set; }

        public int DaysUntilEcpExpires { get; set; }

        public int ActiveTasksCount { get; set; }
        public int OverdueTasksCount { get; set; }

        // Поле для AI-аналитики (например: "Клиент скоро превысит порог НДС!")
        public string AiInsight { get; set; }
    }
}
