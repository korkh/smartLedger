using Application.Common.Helpers.Validators;
using Application.Core;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Storage;

namespace Application.Clients
{
    public class Edit
    {
        public class Command : IRequest<Result<Unit>>
        {
            public ClientDto Client { get; set; }
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
                try
                {
                    // 1. Загружаем клиента со всеми уровнями доступа сразу
                    var client = await _context
                        .Clients.Include(c => c.Internal)
                        .Include(c => c.Sensitive)
                        .FirstOrDefaultAsync(x => x.Id == request.Client.Id, cancellationToken);

                    if (client == null)
                        return Result<Unit>.Failure("Клиент не найден.");

                    // 2. Обновляем базовые поля (Level 1)
                    _mapper.Map(request.Client, client);

                    // 3. Обновляем Level 2 (Internal), если данные пришли и они не пустые
                    // Мы проверяем, не пытается ли пользователь без прав перезаписать данные
                    client.Internal ??= new ClientInternal { ClientId = client.Id };

                    // Логика: если в DTO поле null или маскировано (в зависимости от логики фронта),
                    // мы оставляем старое значение.
                    if (request.Client.ResponsiblePersonContact != null)
                        client.Internal.ResponsiblePersonContact = request
                            .Client
                            .ResponsiblePersonContact;

                    if (request.Client.BankManagerContact != null)
                        client.Internal.BankManagerContact = request.Client.BankManagerContact;

                    if (request.Client.ManagerNotes != null)
                        client.Internal.ManagerNotes = request.Client.ManagerNotes;

                    // 4. Обновляем Level 3 (Sensitive) — Самая важная часть!
                    client.Sensitive ??= new ClientSensitive { ClientId = client.Id };

                    // Логика: обновляем пароли ТОЛЬКО если пришло что-то отличное от маски
                    if (
                        !string.IsNullOrEmpty(request.Client.EcpPassword)
                        && request.Client.EcpPassword != "********"
                    )
                        client.Sensitive.EcpPassword = request.Client.EcpPassword;

                    if (
                        !string.IsNullOrEmpty(request.Client.EsfPassword)
                        && request.Client.EsfPassword != "********"
                    )
                        client.Sensitive.EsfPassword = request.Client.EsfPassword;

                    if (
                        !string.IsNullOrEmpty(request.Client.BankingPasswords)
                        && request.Client.BankingPasswords != "********"
                    )
                        client.Sensitive.BankingPasswords = request.Client.BankingPasswords;

                    if (request.Client.StrategicNotes != null)
                        client.Sensitive.StrategicNotes = request.Client.StrategicNotes;

                    if (request.Client.PersonalInfo != null)
                        client.Sensitive.PersonalInfo = request.Client.PersonalInfo;

                    // 5. Сохраняем изменения
                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                    return result
                        ? Result<Unit>.Success(Unit.Value)
                        : Result<Unit>.Success(Unit.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обновлении клиента {Id}", request.Client.Id);
                    return Result<Unit>.Failure("Системная ошибка при обновлении.");
                }
            }
        }
    }
}
