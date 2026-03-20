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
    public class CreateTransaction
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
            private readonly ILogger<CreateTransaction> _logger;

            public Handler(DataContext context, IMapper mapper, ILogger<CreateTransaction> logger)
            {
                _context = context;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
            {
                _logger.LogInformation(
                    "Создание транзакции для клиента {ClientId}",
                    request.Transaction.ClientId
                );

                try
                {
                    // 1. Загружаем клиента вместе с тарифом
                    var client = await _context
                        .Clients.Include(c => c.CurrentTariff)
                        .FirstOrDefaultAsync(x => x.Id == request.Transaction.ClientId, ct);

                    if (client == null)
                        return Result<Unit>.Failure("Клиент не найден.");

                    // 2. Валидация тарифа (подготовка к будущей логике)
                    if (client.CurrentTariff != null)
                    {
                        // Здесь будет логика: проверяем, не превышен ли лимит операций,
                        // или входит ли категория услуги в текущий пакет.
                        // Пока оставляем место для маневра.
                    }

                    // 3. Маппинг DTO -> Entity
                    var transaction = _mapper.Map<Transaction>(request.Transaction);

                    // 4. Добавление и сохранение
                    _context.Transactions.Add(transaction);

                    var saved = await _context.SaveChangesAsync(ct) > 0;

                    if (!saved)
                        return Result<Unit>.Failure("Не удалось сохранить транзакцию.");

                    return Result<Unit>.Success(Unit.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при создании транзакции");
                    return Result<Unit>.Failure("Системная ошибка при создании транзакции.");
                }
            }
        }
    }
}
