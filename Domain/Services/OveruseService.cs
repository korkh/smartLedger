using Domain.Entities;

namespace Domain.Services
{
    public interface IOveruseService
    {
        void ApplyOveruse(ClientTariff tariff, TariffUsageStats stats);
    }

    public class OveruseService : IOveruseService
    {
        public void ApplyOveruse(ClientTariff tariff, TariffUsageStats stats)
        {
            // Перерасход операций → переносим на следующий месяц
            tariff.CarriedOverOperations = stats.OverusedOperations;

            // Перерасход минут → переносим на следующий месяц
            tariff.CarriedOverMinutes = stats.OverusedMinutes;

            // Здесь можно добавить:
            // - запись в историю
            // - выставление счёта
            // - уведомление менеджера
        }
    }
}
