using API.Services;
using Application.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize(Policy = "Level3Only")]
    public class DashboardController : BaseApiController
    {
        private readonly IaiAnalysisService _aiService;

        public DashboardController(IaiAnalysisService aiService)
        {
            _aiService = aiService;
        }

        // 1. Быстрый метод — только цифры
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClientDashboard(
            Guid id,
            [FromQuery] int? year,
            [FromQuery] int? month
        )
        {
            var result = await Mediator.Send(
                new Dashboard.Query
                {
                    Id = id,
                    Year = year ?? DateTime.Now.Year,
                    Month = month ?? DateTime.Now.Month,
                }
            );
            return HandleResult(result);
        }

        // 2. Метод для ИИ — вызывается по требованию
        [HttpPost("{id}/analyze")]
        public async Task<IActionResult> GetAiInsight(
            Guid id,
            [FromBody] ClientDashboardDto dashboardData
        )
        {
            try
            {
                if (id != dashboardData.Id)
                    return BadRequest("ID mismatch");

                var insight = await _aiService.GetDashboardInsightAsync(dashboardData);
                return Ok(new { insight });
            }
            catch (Exception ex)
            {
                // Это поможет тебе увидеть ошибку в консоли бэкенда
                Console.WriteLine($"Error in AI Analysis: {ex}");
                return StatusCode(500, "Internal Server error during AI analysis");
            }
        }
    }
}
