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
    public class CreateClient
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
            private readonly ILogger<CreateClient> _logger;

            public Handler(DataContext context, IMapper mapper, ILogger<CreateClient> logger)
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
                    "Создание клиента: {First} {Last}",
                    request.Client.FirstName,
                    request.Client.LastName
                );

                try
                {
                    // 1. Проверка дубликата
                    if (!string.IsNullOrEmpty(request.Client.BinIin))
                    {
                        bool exists = await _context.Clients.AnyAsync(
                            x => x.BinIin == request.Client.BinIin,
                            cancellationToken
                        );

                        if (exists)
                            return Result<Unit>.Failure("Клиент с таким БИН/ИИН уже существует.");
                    }

                    // 2. Маппинг
                    var client = _mapper.Map<Client>(request.Client);

                    client.Internal ??= new ClientInternal();
                    client.Sensitive ??= new ClientSensitive();

                    // 3. Начальные транзакции
                    if (request.Transactions?.Count > 0)
                        client.Transactions = _mapper.Map<List<Transaction>>(request.Transactions);

                    // 4. Сохранение
                    _context.Clients.Add(client);

                    bool saved = await _context.SaveChangesAsync(cancellationToken) > 0;

                    if (!saved)
                        return Result<Unit>.Failure("Не удалось сохранить клиента.");

                    return Result<Unit>.Success(Unit.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при создании клиента");
                    return Result<Unit>.Failure("Системная ошибка при создании клиента.");
                }
            }
        }
    }
}
