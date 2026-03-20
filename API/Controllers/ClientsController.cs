using Application.Clients;
using Application.Core;
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

        // ---------------------------------------------------------
        // GET LIST — LEVEL 1+
        // ---------------------------------------------------------
        [Authorize(Policy = "Level1Only")]
        [HttpGet]
        public async Task<IActionResult> GetClients([FromQuery] ClientParams clientParams)
        {
            return HandlePagedResult(
                await Mediator.Send(new GetList.Query { Params = clientParams })
            );
        }

        // ---------------------------------------------------------
        // GET DETAILS — LEVEL 1+
        // ---------------------------------------------------------
        [Authorize(Policy = "Level1Only")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClient(Guid id)
        {
            var result = await Mediator.Send(new Details.Query { Id = id });

            if (!result.IsSuccess || result.Value == null)
                return HandleResult(result);

            var client = result.Value;

            // -----------------------------------------------------
            // LEVEL 1 (Junior_Accountant)
            // -----------------------------------------------------
            if (User.IsInRole("Junior_Accountant"))
            {
                MaskLevel2(client);
                MaskLevel3(client);
            }
            // -----------------------------------------------------
            // LEVEL 2 (Senior_Accountant)
            // -----------------------------------------------------
            else if (User.IsInRole("Senior_Accountant"))
            {
                // Level 2 видит Level 2 данные
                // но НЕ видит Level 3
                MaskLevel3(client);
            }
            // -----------------------------------------------------
            // LEVEL 3 (Admin) — видит всё
            // -----------------------------------------------------

            return HandleResult(Result<ClientDto>.Success(client));
        }

        // ---------------------------------------------------------
        // AI ANALYSIS — LEVEL 2+
        // ---------------------------------------------------------
        [Authorize(Policy = "Level2Only")]
        [HttpPost("{id}/analyze")]
        public async Task<IActionResult> GetAiInsightForClient(Guid id, [FromBody] ClientDto client)
        {
            if (id != client.Id)
                return BadRequest("ID mismatch");

            try
            {
                var insight = await _aiService.GetClientInsightAsync(client);
                return Ok(new { insight });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AI Analysis: {ex}");
                return StatusCode(500, "Internal Server error during AI analysis");
            }
        }

        // ---------------------------------------------------------
        // CREATE — LEVEL 2+
        // ---------------------------------------------------------
        [Authorize(Policy = "Level2Only")]
        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] ClientDto client)
        {
            return HandleResult(await Mediator.Send(new CreateClient.Command { Client = client }));
        }

        // ---------------------------------------------------------
        // EDIT — LEVEL 2+
        // ---------------------------------------------------------
        [Authorize(Policy = "Level2Only")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditClient(Guid id, ClientDto client)
        {
            client.Id = id;
            return HandleResult(await Mediator.Send(new Edit.Command { Client = client }));
        }

        // ---------------------------------------------------------
        // SOFT DELETE — LEVEL 2+
        // ---------------------------------------------------------
        [Authorize(Policy = "Level2Only")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(Guid id)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }));
        }

        // ---------------------------------------------------------
        // HARD DELETE — LEVEL 3 ONLY
        // ---------------------------------------------------------
        [Authorize(Policy = "Level3Only")]
        [HttpDelete("{id}/hard")]
        public async Task<IActionResult> HardDeleteClient(Guid id)
        {
            return HandleResult(await Mediator.Send(new HardDelete.Command { Id = id }));
        }

        // ---------------------------------------------------------
        // MASKING HELPERS
        // ---------------------------------------------------------
        private void MaskLevel2(ClientDto client)
        {
            client.ResponsiblePersonContact = null;
            client.BankManagerContact = null;
            client.ManagerNotes = null;
        }

        private void MaskLevel3(ClientDto client)
        {
            client.EcpPassword = "********";
            client.EsfPassword = "********";
            client.BankingPasswords = "********";
            client.StrategicNotes = null;
            client.PersonalInfo = null;
        }
    }
}
