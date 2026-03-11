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
                            @"Ты — финансовый аналитик бухгалтерской фирмы. 
                            Твоя задача — проанализировать данные клиента.
                            Обязательно проверь:
                            1. 'Хвосты' (поле extraServiceAmount в транзакциях) — если сумма > 0, укажи на                          наличие долга за разовые услуги.
                            2. Лимиты операций — насколько близок клиент к лимиту тарифа.
                            3. Статус НДС и риски.

                            Пиши строго 1-3 предложения на русском языке. Будь конкретен, называй цифры, если они важны. Без приветствий.",
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
