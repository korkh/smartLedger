using Application.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class TransactionsController : BaseApiController
    {
        // ---------------------------------------------------------
        // GET LIST — LEVEL 1+
        // ---------------------------------------------------------
        [Authorize(Policy = "Level1Only")]
        [HttpGet]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] TransactionParams transactionParams
        )
        {
            return HandlePagedResult(
                await Mediator.Send(new GetList.Query { Params = transactionParams })
            );
        }

        // ---------------------------------------------------------
        // GET DETAILS — LEVEL 1+
        // ---------------------------------------------------------
        [Authorize(Policy = "Level1Only")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaction(Guid id)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Id = id }));
        }

        // ---------------------------------------------------------
        // CREATE — LEVEL 2+
        // ---------------------------------------------------------
        [Authorize(Policy = "Level2Only")]
        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionDto transaction)
        {
            return HandleResult(
                await Mediator.Send(new CreateTransaction.Command { Transaction = transaction })
            );
        }

        // ---------------------------------------------------------
        // EDIT — LEVEL 2+
        // ---------------------------------------------------------
        [Authorize(Policy = "Level2Only")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditTransaction(Guid id, TransactionDto transaction)
        {
            transaction.Id = id;

            return HandleResult(
                await Mediator.Send(new Edit.Command { Transaction = transaction })
            );
        }

        // ---------------------------------------------------------
        // SOFT DELETE — LEVEL 2+
        // ---------------------------------------------------------
        // ВАЖНО: SoftDelete для транзакций у тебя НЕ реализован.
        // Я создам корректный маршрут, но он будет работать только
        // если ты добавишь SoftDelete.Handler.
        // ---------------------------------------------------------
        [Authorize(Policy = "Level2Only")]
        [HttpPost("{id}/soft-delete")]
        public async Task<IActionResult> SoftDeleteTransaction(Guid id)
        {
            return HandleResult(await Mediator.Send(new SoftDelete.Command { Id = id }));
        }

        // ---------------------------------------------------------
        // HARD DELETE — LEVEL 3 ONLY
        // ---------------------------------------------------------
        [Authorize(Policy = "Level3Only")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> HardDeleteTransaction(Guid id)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }));
        }
    }
}
