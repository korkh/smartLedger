namespace Application.Interfaces
{
    public interface IAiAnalysisService
    {
        Task<string> GetDashboardInsightAsync(object dashboardData);
        Task<string> GetClientInsightAsync(object clientData);
    }
}
