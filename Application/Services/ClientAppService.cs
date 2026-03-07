using Application.Clients;
using Domain.Services;
using Microsoft.EntityFrameworkCore;
using Storage;

namespace Application.Services
{
    public class ClientAppService
    {
        private readonly DataContext _context;
        private readonly ITaxService _taxService;

        public ClientAppService(DataContext context, ITaxService taxService)
        {
            _context = context;
            _taxService = taxService;
        }

        public async Task<ClientDashboardDto> GetDashboardDataAsync(
            Guid clientId,
            int year,
            int month,
            string currentUserName
        )
        {
            // Подгружаем клиента вместе с транзакциями и текущим тарифом
            var client = await _context
                .Clients.Include(c => c.CurrentTariff)
                .Include(c => c.Transactions)
                    .ThenInclude(t => t.Service)
                // Проверка: ID совпадает И текущий пользователь является ответственным
                .FirstOrDefaultAsync(c =>
                    c.Id == clientId && c.ResponsiblePersonContact.Contains(currentUserName)
                );

            if (client == null)
                return null;

            // 1. Расчет НДС
            var remainingNds = _taxService.GetRemainingNdsLimit(client, year);
            Domain.Constants.TaxConstants.NdsThresholds.TryGetValue(year, out var threshold);
            if (threshold == 0)
                threshold = Domain.Constants.TaxConstants.DefaultNdsThreshold;

            // 2. Расчет Тарифа (Операции и Время)
            var tariffStats = _taxService.GetTariffUsage(client, year, month);

            return new ClientDashboardDto
            {
                Id = client.Id,
                FirstName = client.FirstName,
                LastName = client.LastName,
                TaxRegime = client.TaxRegime,

                // НДС "Спидометр"
                RemainingNdsLimit = remainingNds,
                NdsUsagePercentage =
                    threshold > 0 ? (double)((threshold - remainingNds) / threshold * 100) : 0,

                // Операции "Спидометр"
                RemainingOperations = tariffStats.RemainingOperations,
                OperationsUsagePercentage = tariffStats.OperationsPercentage,

                // Минуты "Спидометр"
                RemainingCommunicationMinutes = tariffStats.RemainingMinutes,
                CommunicationUsagePercentage = tariffStats.MinutesPercentage,
            };
        }
    }
}
