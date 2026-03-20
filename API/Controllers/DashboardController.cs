using Application.Clients;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize(Policy = "Level2Only")]
    public class DashboardController : BaseApiController
    {
        private readonly IAiAnalysisService _aiService;

        public DashboardController(IAiAnalysisService aiService)
        {
            _aiService = aiService;
        }

        // ---------------------------------------------------------
        // 1. Основной дашборд — LEVEL 2+
        // ---------------------------------------------------------
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

        // ---------------------------------------------------------
        // 2. AI анализ дашборда — LEVEL 2+
        // ---------------------------------------------------------
        [HttpPost("{id}/analyze")]
        public async Task<IActionResult> GetAiInsight(
            Guid id,
            [FromBody] ClientDashboardDto dashboardData
        )
        {
            if (id != dashboardData.Id)
                return BadRequest("ID mismatch");

            try
            {
                var insight = await _aiService.GetDashboardInsightAsync(dashboardData);
                return Ok(new { insight });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AI Analysis: {ex}");
                return StatusCode(500, "Internal Server error during AI analysis");
            }
        }
    }
}
