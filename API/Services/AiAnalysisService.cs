using System.Text;
using System.Text.Json;
using API.DTOs;

namespace API.Services
{
    public interface IaiAnalysisService
    {
        Task<string> GetDashboardInsightAsync(object dashboardData);
    }

    public class AiAnalysisService : IaiAnalysisService
    {
        private readonly HttpClient _httpClient;

        public AiAnalysisService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(config["Ollama:Url"] ?? "http://localhost:11434");
        }

        public async Task<string> GetDashboardInsightAsync(object dashboardData)
        {
            var jsonData = JsonSerializer.Serialize(dashboardData);

            var request = new OllamaChatRequest
            {
                messages = new List<OllamaMessage>
                {
                    new()
                    {
                        role = "system",
                        content =
                            "Ты — финансовый аналитик. Пиши строго 1-2 предложения. Без вступлений. Говори только суть по НДС и лимитам. Используй русский язык.",
                    },
                    new() { role = "user", content = $"Проанализируй JSON: {jsonData}" },
                },
            };

            var response = await _httpClient.PostAsync(
                "/api/chat",
                new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                )
            );

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            // В API /chat ответ лежит в message.content
            return doc.RootElement.GetProperty("message").GetProperty("content").GetString();
        }
    }
}
