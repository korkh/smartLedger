using System.Text;
using System.Text.Json;
using API.DTOs;
using Application.Interfaces;

namespace API.Services
{
    public class AiAnalysisService : IAiAnalysisService
    {
        private readonly HttpClient _httpClient;

        public AiAnalysisService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(config["Ollama:Url"] ?? "http://localhost:11434");
        }

        private async Task<string> SendRequestAsync(string systemPrompt, object payload)
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(payload);

                var request = new OllamaChatRequest
                {
                    model = "llama3.2:3b",
                    stream = false,
                    messages = new List<OllamaMessage>
                    {
                        new() { role = "system", content = systemPrompt },
                        new() { role = "user", content = jsonData },
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

                if (!response.IsSuccessStatusCode)
                    return $"Ошибка ИИ: {response.StatusCode}";

                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);

                if (doc.RootElement.TryGetProperty("message", out var messageElement))
                    return messageElement.GetProperty("content").GetString() ?? "Нет контента";

                return "Ошибка: пустой ответ от ИИ.";
            }
            catch (Exception ex)
            {
                return $"Ошибка ИИ: {ex.Message}";
            }
        }

        public Task<string> GetDashboardInsightAsync(object dashboardData)
        {
            const string prompt =
                @"Ты — ведущий финансовый эксперт SmartLedger. 
                Анализируй данные дашборда по правилам: 
                1) НДС, 2) Тариф, 3) Долги, 4) ЭЦП, 5) Задачи. 
                Пиши 2–3 предложения, строго по делу.";

            return SendRequestAsync(prompt, dashboardData);
        }

        public Task<string> GetClientInsightAsync(object clientData)
        {
            const string prompt =
                @"Ты — финансовый аналитик SmartLedger. 
                Дай краткое резюме по клиенту: тариф, обороты, НДС, долги, ЭЦП. 
                Максимум 3 предложения.";

            return SendRequestAsync(prompt, clientData);
        }
    }
}
