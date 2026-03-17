namespace Application.Interfaces
{
    public interface IAiRiskAnalysisService
    {
        Task<string> AnalyzeRiskAsync(object riskData);
        Task<RiskAnalysisResult> AnalyzeRiskStructuredAsync(object riskData);
        Task<RiskForecastResult> ForecastRiskAsync(object data);
    }

    public class RiskAnalysisResult
    {
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; }
        public string RiskColor { get; set; }
        public string Recommendations { get; set; }
        public string Summary { get; set; }
    }

    public class RiskForecastResult
    {
        public int Month1Score { get; set; }
        public int Month2Score { get; set; }
        public int Month3Score { get; set; }
        public string Summary { get; set; }
    }
}
