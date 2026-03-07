using Application.Common.Helpers.Validators;
using Application.Core;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Storage;

namespace Application.Transactions
{
    public class Edit
    {
        public class Command : IRequest<Result<Unit>>
        {
            public TransactionDto Transaction { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Transaction).SetValidator(new TransactionValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly ILogger<Edit> _logger;

            public Handler(DataContext context, IMapper mapper, ILogger<Edit> logger)
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
                // Пытаемся распарсить ID из DTO (так как в DTO он Guid/string, а в базе int)
                var transactionId = request.Transaction.Id;

                _logger.LogInformation(
                    "Request to edit transaction ID: {Id} for client: {Client}",
                    transactionId,
                    request.Transaction.ClientName
                );

                try
                {
                    // 1. Находим транзакцию в БД
                    var transaction = await _context.Transactions.FirstOrDefaultAsync(
                        x => x.Id == transactionId,
                        cancellationToken
                    );

                    if (transaction == null)
                    {
                        _logger.LogWarning("Транзакция с ID {Id} не найдена", transactionId);
                        return null;
                    }

                    // 2. Обновляем поля сущности данными из DTO
                    // AutoMapper обновит только разрешенные поля (дата, количество, минуты и т.д.)
                    _mapper.Map(request.Transaction, transaction);

                    // 3. Сохраняем изменения
                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                    if (!result)
                    {
                        _logger.LogInformation(
                            "Транзакция {Id} не была изменена (данные идентичны)",
                            transactionId
                        );
                        return Result<Unit>.Success(Unit.Value);
                    }

                    _logger.LogInformation("Транзакция {Id} успешно обновлена", transactionId);
                    return Result<Unit>.Success(Unit.Value);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(
                        ex,
                        "Конфликт параллельного доступа при обновлении транзакции {Id}",
                        transactionId
                    );
                    return Result<Unit>.Failure(
                        "Данные были изменены другим пользователем. Пожалуйста, обновите страницу."
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обновлении транзакции {Id}", transactionId);
                    return Result<Unit>.Failure(
                        "Произошла системная ошибка при сохранении изменений."
                    );
                }
            }
        }
    }
}
