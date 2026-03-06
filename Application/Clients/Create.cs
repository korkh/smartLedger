using Application.Common.Helpers.Validators;
using Application.Core;
using Application.Transactions;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Storage;

namespace Application.Clients
{
    public class Create
    {
        public class Command : IRequest<Result<Unit>>
        {
            public ClientDto Client { get; set; }
            public List<TransactionDto> Transactions { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Client).SetValidator(new ClientValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly ILogger<Create> _logger; // Added logger

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
                    "Попытка создания нового клиента: {FirstName} {LastName}",
                    request.Client.FirstName,
                    request.Client.LastName
                );

                try
                {
                    // 1. Проверка на дубликат по БИН/ИИН
                    if (!string.IsNullOrEmpty(request.Client.BinIin))
                    {
                        var exists = await _context.Clients.AnyAsync(
                            x => x.BinIin == request.Client.BinIin,
                            cancellationToken
                        );

                        if (exists)
                        {
                            _logger.LogWarning(
                                "Клиент с БИН/ИИН {BinIin} уже существует",
                                request.Client.BinIin
                            );
                            return Result<Unit>.Failure(
                                "Клиент с таким БИН/ИИН уже зарегистрирован в системе."
                            );
                        }
                    }

                    // 2. Маппинг данных
                    var client = _mapper.Map<Client>(request.Client);

                    if (request.Transactions != null && request.Transactions.Count != 0)
                    {
                        _logger.LogInformation(
                            "Добавление {Count} начальных транзакций для клиента",
                            request.Transactions.Count
                        );
                        client.Transactions = _mapper.Map<List<Transaction>>(request.Transactions);
                    }

                    // 3. Сохранение
                    _context.Clients.Add(client);
                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                    if (!result)
                    {
                        _logger.LogError(
                            "Ошибка при записи клиента в базу данных. SaveChanges вернул 0."
                        );
                        return Result<Unit>.Failure(
                            "Не удалось сохранить данные клиента в базе данных."
                        );
                    }

                    _logger.LogInformation("Клиент успешно создан. ID: {Id}", client.Id);
                    return Result<Unit>.Success(Unit.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Критическая ошибка при создании клиента {LastName}",
                        request.Client.LastName
                    );
                    return Result<Unit>.Failure(
                        "Произошла внутренняя ошибка сервера при создании клиента."
                    );
                }
            }
        }
    }
}
