using Domain.Entities;

namespace Application.Interfaces
{
    public interface IRiskAlertService
    {
        Task CheckAndNotifyAsync(Client client, RiskAnalysisResult risk);
    }
}
