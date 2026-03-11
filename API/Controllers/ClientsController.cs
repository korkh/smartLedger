using Application.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ClientsController : BaseApiController
    {
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
