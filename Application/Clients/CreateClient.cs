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
                        if (
                            await _context.Clients.AnyAsync(
                                x => x.BinIin == request.Client.BinIin,
                                cancellationToken
                            )
                        )
                            return Result<Unit>.Failure("Клиент с таким БИН/ИИН уже существует.");
                    }

                    // 2. Создаем сущность Client
                    var client = _mapper.Map<Client>(request.Client);

                    // 3. Инициализируем и наполняем связанные сущности вручную или через маппер
                    // Это гарантирует, что 1-к-1 связи будут созданы одновременно
                    client.Internal = new ClientInternal
                    {
                        ClientId = client.Id,
                        ResponsiblePersonContact = request.Client.ResponsiblePersonContact,
                        BankManagerContact = request.Client.BankManagerContact,
                        ManagerNotes = request.Client.ManagerNotes,
                    };

                    client.Sensitive = new ClientSensitive
                    {
                        ClientId = client.Id,
                        EcpPassword = request.Client.EcpPassword,
                        EsfPassword = request.Client.EsfPassword,
                        BankingPasswords = request.Client.BankingPasswords,
                        StrategicNotes = request.Client.StrategicNotes,
                        PersonalInfo = request.Client.PersonalInfo,
                    };

                    // 4. Начальные транзакции
                    if (request.Transactions?.Count > 0)
                        client.Transactions = _mapper.Map<List<Transaction>>(request.Transactions);

                    // 5. Сохранение (EF Core сам поймет, что нужно добавить 3 записи в разные таблицы)
                    _context.Clients.Add(client);

                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                    return result
                        ? Result<Unit>.Success(Unit.Value)
                        : Result<Unit>.Failure("Ошибка сохранения");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при создании клиента");
                    return Result<Unit>.Failure("Системная ошибка.");
                }
            }
        }
    }
}
