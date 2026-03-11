using API.Services;
using Application.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize(Roles = "Senior_Accountant, Admin")]
    public class DashboardController : BaseApiController
    {
        private readonly IaiAnalysisService _aiService;

        public DashboardController(IaiAnalysisService aiService)
        {
            _aiService = aiService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClientDashboard(
            Guid id,
            [FromQuery] int? year,
            [FromQuery] int? month
        )
        {
            // 1. Получаем данные через MediatR
            var result = await Mediator.Send(
                new Dashboard.Query
                {
                    Id = id,
                    Year = year ?? DateTime.Now.Year,
                    Month = month ?? DateTime.Now.Month,
                }
            );

            // 2. Если данные найдены, запрашиваем инсайт у ИИ
            if (result.IsSuccess && result.Value != null)
            {
                // Передаем весь объект DTO в Ollama для анализа
                result.Value.AiInsight = await _aiService.GetDashboardInsightAsync(result.Value);
            }

            return HandleResult(result);
        }
    }
}
