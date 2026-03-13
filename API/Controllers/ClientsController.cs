using API.Services;
using Application.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ClientsController(IaiAnalysisService aiService) : BaseApiController
    {
        private readonly IaiAnalysisService _aiService = aiService;

        [Authorize(Policy = "Level1Only")]
        [HttpGet]
        public async Task<IActionResult> GetClients([FromQuery] ClientParams clientParams)
        {
            return HandlePagedResult(
                await Mediator.Send(new GetList.Query { Params = clientParams })
            );
        }

        [Authorize(Policy = "Level1Only")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClient(Guid id)
        {
            var result = await Mediator.Send(new Details.Query { Id = id });

            if (result.IsSuccess && result.Value != null)
            {
                var client = result.Value;

                // 1. Если это Junior (Level 1) — скрываем ВСЁ (пароли и заметки 2/3 уровней)
                if (!User.IsInRole("Senior_Accountant") && !User.IsInRole("Admin"))
                {
                    client.EcpPassword = "********";
                    client.EsfPassword = "********";
                    client.BankingPasswords = "********";
                    client.ManagerNotes = null; // Заметки 2 уровня
                    client.StrategicNotes = null;
                    client.PersonalInfo = null;
                }
                // 2. Если это Senior (Level 2), но НЕ Admin
                else if (User.IsInRole("Senior_Accountant") && !User.IsInRole("Admin"))
                {
                    // Пароли ОСТАВЛЯЕМ (они есть в файле 2 уровня)

                    // Скрываем только то, что появляется исключительно в 3 уровне:
                    client.StrategicNotes = null;
                    client.PersonalInfo = null;
                }

                // 3. Если Admin (Level 3) — ничего не скрываем, видит всё.
            }

            return HandleResult(result);
        }

        // 2. Метод для ИИ — вызывается по требованию
        [Authorize(Policy = "Level2Only")]
        [HttpPost("{id}/analyze")]
        public async Task<IActionResult> GetAiInsightForClient(Guid id, [FromBody] ClientDto client)
        {
            try
            {
                if (id != client.Id)
                    return BadRequest("ID mismatch");

                var insight = await _aiService.GetClientInsightAsync(client);
                return Ok(new { insight });
            }
            catch (Exception ex)
            {
                // Это поможет тебе увидеть ошибку в консоли бэкенда
                Console.WriteLine($"Error in AI Analysis: {ex}");
                return StatusCode(500, "Internal Server error during AI analysis");
            }
        }

        [Authorize(Policy = "Level2Only")]
        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] ClientDto client)
        {
            //No need !ModelState.IsValid due to usage ApiController attribute
            return HandleResult(await Mediator.Send(new CreateClient.Command { Client = client }));
        }

        [Authorize(Policy = "Level2Only")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditClient(Guid id, ClientDto client)
        {
            client.Id = id;
            return HandleResult(await Mediator.Send(new Edit.Command { Client = client }));
        }

        [Authorize(Policy = "Level2Only")]
        [HttpDelete("{id}")] // DELETE /api/clients/{id}
        public async Task<IActionResult> DeleteClient(Guid id)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }));
        }

        /// Окончательное удаление из базы (Hard Delete).
        /// Доступно ТОЛЬКО Уровню 3 (Admin).
        [Authorize(Policy = "Level3Only")]
        [HttpDelete("{id}/hard")] // DELETE /api/clients/{id}/hard
        public async Task<IActionResult> HardDeleteClient(Guid id)
        {
            return HandleResult(await Mediator.Send(new HardDelete.Command { Id = id }));
        }
    }
}
