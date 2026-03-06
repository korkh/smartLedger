using Application.Common.Helpers.Validators;
using Application.Core;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Storage;

namespace Application.Transactions
{
    public class Create
    {
        public class Command : IRequest<Result<Unit>>
        {
            public TransactionDto Transaction { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                // Reusing the TransactionValidator we fixed earlier
                RuleFor(x => x.Transaction).SetValidator(new TransactionValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly ILogger<Create> _logger;

            public Handler(DataContext context, IMapper mapper, ILogger<Create> logger)
            {
                _context = context;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<Result<Unit>> Handle(
                Command request,
                CancellationToken cancellationToken
            )
            {
                _logger.LogInformation(
                    "Начало процесса создания транзакции для клиента: {ClientId}",
                    request.Transaction.ClientId
                );

                try
                {
                    // 1. Проверка существования клиента (у тебя в базе Id - int, а в DTO - Guid,
                    // проверь соответствие типов в БД. Если в БД Guid, используй его)
                    var clientExists = await _context.Clients.AnyAsync(
                        x => x.Id.ToString() == request.Transaction.ClientId.ToString(),
                        cancellationToken
                    );

                    if (!clientExists)
                    {
                        _logger.LogWarning(
                            "Попытка создать транзакцию для несуществующего клиента: {ClientId}",
                            request.Transaction.ClientId
                        );
                        return Result<Unit>.Failure("Указанный клиент не найден в системе.");
                    }

                    // 2. Маппинг DTO в сущность
                    var transaction = _mapper.Map<Transaction>(request.Transaction);

                    // 3. Добавление в контекст
                    _context.Transactions.Add(transaction);

                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                    if (!result)
                    {
                        _logger.LogError("Транзакция не была сохранена в базе данных.");
                        return Result<Unit>.Failure("Не удалось сохранить транзакцию.");
                    }

                    _logger.LogInformation(
                        "Транзакция успешно создана. ID: {TransactionId}",
                        transaction.Id
                    );
                    return Result<Unit>.Success(Unit.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Критическая ошибка при создании транзакции.");
                    return Result<Unit>.Failure(
                        "Произошла внутренняя ошибка при создании транзакции."
                    );
                }
            }
        }
    }
}
