using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Storage;

namespace Application.Transactions
{
    public class Delete
    {
        public class Command : IRequest<Result<Unit>>
        {
            public int Id { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly ILogger<Delete> _logger;

            public Handler(DataContext context, ILogger<Delete> logger)
            {
                _context = context;
                _logger = logger;
            }

            public async Task<Result<Unit>> Handle(
                Command request,
                CancellationToken cancellationToken
            )
            {
                _logger.LogInformation("Попытка удаления транзакции с ID: {Id}", request.Id);

                try
                {
                    // 1. Поиск транзакции
                    var transaction = await _context.Transactions.FirstOrDefaultAsync(
                        x => x.Id == request.Id,
                        cancellationToken
                    );

                    if (transaction == null)
                    {
                        _logger.LogWarning(
                            "Транзакция с ID {Id} не найдена в базе данных",
                            request.Id
                        );
                        return null; // Will result in 404 NotFound in Controller
                    }

                    // 2. Удаление
                    _context.Transactions.Remove(transaction);

                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                    if (!result)
                    {
                        _logger.LogError(
                            "Не удалось зафиксировать удаление транзакции {Id} в БД",
                            request.Id
                        );
                        return Result<Unit>.Failure(
                            "Ошибка при удалении транзакции из базы данных."
                        );
                    }

                    _logger.LogInformation("Транзакция {Id} успешно удалена", request.Id);
                    return Result<Unit>.Success(Unit.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Критическая ошибка при удалении транзакции {Id}",
                        request.Id
                    );
                    return Result<Unit>.Failure(
                        "Произошла системная ошибка при удалении транзакции."
                    );
                }
            }
        }
    }
}
