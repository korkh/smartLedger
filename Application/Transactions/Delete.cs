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
            public Guid Id { get; set; }
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

            public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
            {
                try
                {
                    var affected = await _context
                        .Transactions.Where(x => x.Id == request.Id)
                        .ExecuteDeleteAsync(ct);

                    if (affected == 0)
                        return Result<Unit>.Failure("Транзакция не найдена.");

                    return Result<Unit>.Success(Unit.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при удалении транзакции");
                    return Result<Unit>.Failure("Системная ошибка при удалении транзакции.");
                }
            }
        }
    }
}
