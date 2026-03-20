using Application.Core;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Storage;

namespace Application.Clients
{
    public class EditSensitive
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid ClientId { get; set; }
            public string StrategicNotes { get; set; }
            public string PersonalInfo { get; set; }
            public string EcpPassword { get; set; }
            public string EsfPassword { get; set; }
            public string BankingPasswords { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            private readonly ILogger<EditSensitive> _logger;

            public Handler(
                DataContext context,
                IUserAccessor userAccessor,
                ILogger<EditSensitive> logger
            )
            {
                _context = context;
                _userAccessor = userAccessor;
                _logger = logger;
            }

            public async Task<Result<Unit>> Handle(
                Command request,
                CancellationToken cancellationToken
            )
            {
                // 1. Проверка прав (уже есть, это отлично)
                if (!_userAccessor.IsAdmin())
                {
                    _logger.LogWarning(
                        "Попытка несанкционированного доступа к Sensitive данным. Пользователь: {User}",
                        _userAccessor.GetUserName()
                    );
                    return Result<Unit>.Failure("Недостаточно прав.");
                }

                var client = await _context
                    .Clients.Include(x => x.Sensitive)
                    .FirstOrDefaultAsync(x => x.Id == request.ClientId, cancellationToken);

                if (client == null)
                    return Result<Unit>.Failure("Клиент не найден.");

                // 2. Инициализация, если данных еще нет
                if (client.Sensitive == null)
                {
                    client.Sensitive = new ClientSensitive { ClientId = client.Id };
                }

                // 3. Умное обновление паролей (защита от "звездочек")
                // Обновляем только если пришло значение и это не маска
                if (!string.IsNullOrEmpty(request.EcpPassword) && request.EcpPassword != "********")
                    client.Sensitive.EcpPassword = request.EcpPassword;

                if (!string.IsNullOrEmpty(request.EsfPassword) && request.EsfPassword != "********")
                    client.Sensitive.EsfPassword = request.EsfPassword;

                if (
                    !string.IsNullOrEmpty(request.BankingPasswords)
                    && request.BankingPasswords != "********"
                )
                    client.Sensitive.BankingPasswords = request.BankingPasswords;

                // 4. Текстовые поля обновляем как есть
                client.Sensitive.StrategicNotes = request.StrategicNotes;
                client.Sensitive.PersonalInfo = request.PersonalInfo;

                // 5. Логируем факт изменения (Audit Trail)
                _logger.LogInformation(
                    "Пользователь {Admin} изменил чувствительные данные клиента {ClientId}",
                    _userAccessor.GetUserName(),
                    client.Id
                );

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                // Если админ нажал сохранить, но ничего не менял — возвращаем Success
                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
