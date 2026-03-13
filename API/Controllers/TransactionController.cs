using Application.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class TransactionsController : BaseApiController
    {
        [Authorize(Policy = "Level1Only")]
        [HttpGet]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] TransactionParams transactionParams
        )
        {
            // Используем HandlePagedResult для автоматической обработки пагинации и заголовков
            return HandlePagedResult(
                await Mediator.Send(new GetList.Query { Params = transactionParams })
            );
        }

        [Authorize(Policy = "Level1Only")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaction(Guid id)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Id = id }));
        }

        [Authorize(Policy = "Level2Only")]
        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionDto transaction)
        {
            return HandleResult(
                await Mediator.Send(new CreateTransaction.Command { Transaction = transaction })
            );
        }

        [Authorize(Policy = "Level2Only")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditTransaction(Guid id, TransactionDto transaction)
        {
            transaction.Id = id;
            return HandleResult(
                await Mediator.Send(new Edit.Command { Transaction = transaction })
            );
        }

        [Authorize(Policy = "Level2Only")]
        [HttpPost("{id}/soft-delete")] // Используем POST, так как это действие/команда
        public async Task<IActionResult> SoftDeleteTransaction(Guid id)
        {
            return HandleResult(await Mediator.Send(new SoftDelete.Command { Id = id }));
        }

        [Authorize(Policy = "Level3Only")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> HardDeleteTransaction(Guid id)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }));
        }
    }
}
