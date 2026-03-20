using Application.Core;
using Domain.Interfaces;
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
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly ILogger<Delete> _logger;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, ILogger<Delete> logger, IUserAccessor userAccessor)
            {
                _context = context;
                _logger = logger;
                _userAccessor = userAccessor;
            }

            public async Task<Result<Unit>> Handle(
                Command request,
                CancellationToken cancellationToken
            )
            {
                try
                {
                    // Только Admin или Senior
                    if (!_userAccessor.IsAdmin() && !_userAccessor.IsSeniorAccountant())
                        return Result<Unit>.Failure("Недостаточно прав для удаления клиента.");

                    var client = await _context.Clients.FirstOrDefaultAsync(
                        x => x.Id == request.Id,
                        cancellationToken
                    );

                    if (client == null)
                        return Result<Unit>.Failure("Клиент не найден.");

                    var hasTransactions = await _context.Transactions.AnyAsync(
                        t => t.ClientId == request.Id,
                        cancellationToken
                    );

                    if (hasTransactions)
                        return Result<Unit>.Failure(
                            "Нельзя удалить клиента с историей транзакций. Используйте архивацию."
                        );

                    _context.Clients.Remove(client);

                    bool saved = await _context.SaveChangesAsync(cancellationToken) > 0;

                    if (!saved)
                        return Result<Unit>.Failure("Ошибка при удалении клиента.");

                    return Result<Unit>.Success(Unit.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при удалении клиента");
                    return Result<Unit>.Failure("Системная ошибка при удалении клиента.");
                }
            }
        }
    }
}
