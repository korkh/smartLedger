using System.Text;
using System.Text.Json;
using API.DTOs;

namespace API.Services
{
    public interface IaiAnalysisService
    {
        Task<string> GetDashboardInsightAsync(object dashboardData);
        Task<string> GetClientInsightAsync(object clientData);
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
            try
            {
                var jsonData = JsonSerializer.Serialize(dashboardData);

                var request = new OllamaChatRequest
                {
                    model = "llama3.2:3b",
                    stream = false,
                    messages = new List<OllamaMessage>
                    {
                        new()
                        {
                            role = "system",
                            content =
                                @"Ты — ведущий финансовый эксперт. Анализируй данные клиента SmartLedger.
                
                КРИТЕРИИ АНАЛИЗА:
                1. НДС: Порог 30 000 МРП. Если оборот (currentYearTurnover) > 80% от порога (ndsThreshold), предупреди о риске постановки на учет.
                2. ТАРИФ: Если фактические операции (operationsActual) превышают лимит (operationsLimit), рекомендуй переход на тариф выше.
                3. ФИНАНСЫ: Если есть общая задолженность (totalOutstandingDebt > 0), укажи точную сумму к взысканию.
                4. ЭЦП: Если до истечения (daysUntilEcpExpires) меньше 14 дней, это критический приоритет.
                5. ЗАДАЧИ: Если есть просроченные задачи (overdueTasksCount), акцентируй на них внимание.

                ФОРМАТ ОТВЕТА:
                - Только 2-3 предложения. 
                - Никакой воды и приветствий. 
                - Используй профессиональный, но лаконичный тон.
                - Указывай конкретные суммы и проценты.
                - НИКОГДА не упоминай названия технических полей JSON (например, 'extraServiceAmount' или 'currentYearTurnover'). Пиши человеческим языком: 'задолженность по доп. услугам', 'годовой оборот' и т.д.
                КРИТЕРИИ ФИНАНСОВ:
                - totalToPay: это СВЕЖИЙ счет только за текущий месяц (Тариф + Доп.услуги).
                - totalOutstandingDebt: это СТАРАЯ задолженность клиента (дебиторка).
                ИНСТРУКЦИЯ: Если есть totalOutstandingDebt, пиши: 'Внимание, имеется задолженность за прошлые периоды: [сумма]'. 
                Если есть totalToPay, пиши: 'Сумма к оплате за текущий месяц: [сумма]'.
                - НИКОГДА не складывай эти числа в одно, называй их раздельно.",
                        },
                        new() { role = "user", content = $"Данные дашборда: {jsonData}" },
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
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return $"Ошибка ИИ: сервер вернул статус {response.StatusCode}";
                }

                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);

                // Важно: в Ollama /api/chat ответ лежит в message -> content
                if (doc.RootElement.TryGetProperty("message", out var messageElement))
                {
                    return messageElement.GetProperty("content").GetString() ?? "Нет контента";
                }

                return "Не удалось прочитать ответ от ИИ.";
            }
            catch (HttpRequestException ex)
            {
                // Если Ollama не запущена вообще
                return $"Ошибка: Не удалось подключиться к серверу Ollama. Убедитесь, что он запущен. {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Ошибка анализа: {ex.Message}";
            }
        }

        public async Task<string> GetClientInsightAsync(object clientData)
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(clientData);

                var request = new OllamaChatRequest
                {
                    model = "llama3.2:3b",
                    stream = false,
                    messages = new List<OllamaMessage>
                    {
                        new()
                        {
                            role = "system",
                            content =
                                @"Ты — финансовый аналитик системы SmartLedger. Твоя задача: провести аудит карточки конкретного клиента.
        
        ПРАВИЛА АНАЛИЗА:
        1. ТАРИФ И ЛИМИТЫ: Сравни количество фактических операций с лимитом в тарифе. Если превышение > 10%, предложи переход на следующий пакет.
        2. ОБОРОТЫ И НДС: Порог постановки на учет по НДС в Казахстане — 30 000 МРП (примерно 110 млн ₸). Если текущий оборот клиента превысил 25 000 МРП, выдели это как критический риск.
        3. ДОЛГИ: Четко разделяй: 'текущий счет' (за этот месяц) и 'дебиторская задолженность' (просрочка за прошлые периоды). 
        4. БЕЗОПАСНОСТЬ: Проверь срок действия ЭЦП. Если осталось менее 10 дней, требуй немедленного продления.
        5. ДОП. ДОХОД: Если клиент часто заказывает услуги вне тарифа (extraServiceAmount), отметь это как повод для обсуждения индивидуальных условий.

        ТРЕБОВАНИЯ К ОТВЕТУ:
        - Максимум 3 коротких, емких предложения.
        - Никаких вводных слов ('Исходя из данных...', 'Я вижу...'). Сразу к делу.
        - Используй только человеческие названия: вместо 'binIin' — 'ИИН', вместо 'ecpExpiryDate' — 'срок ЭЦП'.
        - Если данных по какому-то пункту нет, просто пропусти его.
        - Тон: деловой, предупреждающий.",
                        },
                        new()
                        {
                            role = "user",
                            content =
                                $"Проанализируй данные этого клиента и дай краткое резюме: {jsonData}",
                        },
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
                    return "Не удалось получить анализ (сервер ИИ недоступен).";

                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);

                if (doc.RootElement.TryGetProperty("message", out var messageElement))
                {
                    return messageElement.GetProperty("content").GetString() ?? "Анализ пуст.";
                }

                return "Ошибка обработки ответа ИИ.";
            }
            catch (Exception ex)
            {
                return $"Ошибка связи с ИИ: {ex.Message}";
            }
        }
    }
}
