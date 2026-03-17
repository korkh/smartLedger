using System.Text;
using System.Text.Json;
using API.DTOs;
using Application.Interfaces;

namespace API.Services
{
    public class AiRiskAnalysisService : IAiRiskAnalysisService
    {
        private readonly HttpClient _httpClient;

        public AiRiskAnalysisService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(config["Ollama:Url"] ?? "http://localhost:11434");
        }

        private async Task<string> SendPromptAsync(string systemPrompt, object payload)
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

        public Task<string> AnalyzeRiskAsync(object riskData)
        {
            const string prompt =
                @"
Ты — эксперт по рискам SmartLedger. 
Проанализируй данные клиента по следующим критериям:

1. НДС: 
   - если годовой оборот > 80% порога — высокий риск постановки на учёт.
   - если > 90% — критический риск.

2. Долги:
   - если есть задолженность за прошлые периоды — высокий риск.
   - если сумма > 500 000 ₸ — критический риск.

3. Отчётность:
   - если есть просроченные отчёты — высокий риск.
   - если просрочка > 30 дней — критический риск.

4. ЭЦП:
   - если до истечения < 14 дней — высокий риск.
   - если < 7 дней — критический риск.

5. Тариф:
   - если превышение операций > 20% — риск.
   - если превышение > 50% — критический риск.

Формат ответа:
- 3–4 коротких предложения.
- Укажи конкретные риски и их уровень.
- Пиши строго по делу, без воды.";

            return SendPromptAsync(prompt, riskData);
        }

        public async Task<RiskAnalysisResult> AnalyzeRiskStructuredAsync(object riskData)
        {
            const string prompt =
                @"
Ты — эксперт по рискам SmartLedger. 
Проанализируй данные клиента и верни строго JSON следующего формата:

{
  ""riskScore"": 0–100,
  ""riskLevel"": ""Low | Medium | High | Critical"",
  ""riskColor"": ""Green | Yellow | Orange | Red"",
  ""recommendations"": ""краткие советы"",
  ""summary"": ""2–3 предложения о рисках""
}

Правила оценки:
- НДС: >80% порога = High, >90% = Critical
- Долги: >0 = Medium, >500000 = High
- Просроченные задачи: >0 = Medium, >5 = High
- ЭЦП: <14 дней = Medium, <7 дней = High
- Тариф: превышение >20% = Medium, >50% = High

Верни ТОЛЬКО JSON.";

            var json = await SendPromptAsync(prompt, riskData);

            try
            {
                return JsonSerializer.Deserialize<RiskAnalysisResult>(json)
                    ?? new RiskAnalysisResult { Summary = "Ошибка парсинга JSON" };
            }
            catch
            {
                return new RiskAnalysisResult { Summary = "ИИ вернул некорректный JSON" };
            }
        }

        public async Task<RiskForecastResult> ForecastRiskAsync(object data)
        {
            const string prompt =
                @"
Ты — эксперт по прогнозированию рисков SmartLedger.
Проанализируй данные клиента и верни прогноз риска на 3 месяца вперёд.

Формат ответа строго JSON:
{
  ""month1Score"": 0–100,
  ""month2Score"": 0–100,
  ""month3Score"": 0–100,
  ""summary"": ""краткое описание динамики""
}

Учитывай:
- рост оборотов
- рост задолженности
- ухудшение отчётности
- приближение ЭЦП
- превышение тарифа
";

            var json = await SendPromptAsync(prompt, data);

            return JsonSerializer.Deserialize<RiskForecastResult>(json)
                ?? new RiskForecastResult { Summary = "Ошибка парсинга JSON" };
        }
    }
}
