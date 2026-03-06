using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Storage;

namespace Application.Clients
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
                _logger.LogInformation("Попытка удаления клиента с ID: {Id}", request.Id);

                try
                {
                    // 1. Поиск клиента с загрузкой связанных транзакций для проверки
                    var client = await _context
                        .Clients.Include(c => c.Transactions)
                        .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                    if (client == null)
                    {
                        _logger.LogWarning("Клиент с ID {Id} не найден для удаления", request.Id);
                        return null;
                    }

                    // 2. Проверка наличия транзакций
                    if (client.Transactions != null && client.Transactions.Any())
                    {
                        _logger.LogWarning(
                            "Отказ в удалении: у клиента {Id} есть {Count} транзакций",
                            request.Id,
                            client.Transactions.Count
                        );

                        return Result<Unit>.Failure(
                            "Нельзя удалить клиента с существующими транзакциями. Сначала удалите транзакции."
                        );
                    }

                    // 3. Удаление
                    _context.Remove(client);

                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                    if (!result)
                    {
                        _logger.LogError(
                            "Ошибка при выполнении SaveChanges для удаления клиента {Id}",
                            request.Id
                        );
                        return Result<Unit>.Failure("Ошибка при удалении клиента из базы данных.");
                    }

                    _logger.LogInformation("Клиент с ID {Id} успешно удален", request.Id);
                    return Result<Unit>.Success(Unit.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Исключение при попытке удаления клиента {Id}",
                        request.Id
                    );
                    return Result<Unit>.Failure("Произошла системная ошибка при удалении клиента.");
                }
            }
        }
    }
}
