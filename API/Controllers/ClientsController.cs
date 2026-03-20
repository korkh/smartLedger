using Application.Clients;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ClientsController : BaseApiController
    {
        private readonly IAiAnalysisService _aiService;

        public ClientsController(IAiAnalysisService aiService)
        {
            _aiService = aiService;
        }

        // --- GET LIST (Level 1+) ---
        [Authorize(Policy = "Level1Only")]
        [HttpGet]
        public async Task<IActionResult> GetClients([FromQuery] ClientParams clientParams)
        {
            return HandlePagedResult(
                await Mediator.Send(new GetList.Query { Params = clientParams })
            );
        }

        // --- GET DETAILS (Level 1+) ---
        [Authorize(Policy = "Level1Only")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClient(Guid id)
        {
            // Handler сам определит роль пользователя через IUserAccessor
            // и вернет ClientDto с уже скрытыми полями.
            return HandleResult(await Mediator.Send(new Details.Query { Id = id }));
        }

        // --- AI ANALYSIS (Level 2+) ---
        [Authorize(Policy = "Level2Only")]
        [HttpPost("{id}/analyze")]
        public async Task<IActionResult> GetAiInsightForClient(Guid id)
        {
            // Сначала получаем данные клиента через Mediator (безопасно)
            var result = await Mediator.Send(new Details.Query { Id = id });

            if (!result.IsSuccess)
                return HandleResult(result);

            try
            {
                // Отправляем проверенные данные из БД в сервис AI
                var insight = await _aiService.GetClientInsightAsync(result.Value);
                return Ok(new { insight });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AI Analysis: {ex}");
                return StatusCode(500, "Ошибка AI анализа");
            }
        }

        // --- CREATE (Level 2+) ---
        [Authorize(Policy = "Level2Only")]
        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] ClientDto client)
        {
            return HandleResult(await Mediator.Send(new CreateClient.Command { Client = client }));
        }

        // --- EDIT (Level 2+) ---
        [Authorize(Policy = "Level2Only")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditClient(Guid id, ClientDto client)
        {
            client.Id = id;
            return HandleResult(await Mediator.Send(new Edit.Command { Client = client }));
        }

        // --- SOFT DELETE (Level 2+) ---
        [Authorize(Policy = "Level2Only")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(Guid id)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }));
        }

        // --- HARD DELETE (Level 3 ONLY) ---
        [Authorize(Policy = "Level3Only")]
        [HttpDelete("{id}/hard")]
        public async Task<IActionResult> HardDeleteClient(Guid id)
        {
            return HandleResult(await Mediator.Send(new HardDelete.Command { Id = id }));
        }
    }
}
