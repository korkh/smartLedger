using API.Services;
using Application.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ClientsController(IaiAnalysisService aiService) : BaseApiController
    {
        private readonly IaiAnalysisService _aiService = aiService;

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetClients([FromQuery] ClientParams clientParams)
        {
            return HandlePagedResult(
                await Mediator.Send(new GetList.Query { Params = clientParams })
            );
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClient(Guid id)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Id = id }));
        }

        // 2. Метод для ИИ — вызывается по требованию
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

        [Authorize(Policy = "Level3Only")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(Guid id)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }));
        }
    }
}
