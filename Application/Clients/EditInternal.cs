using Application.Core;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Storage;

namespace Application.Clients
{
    public class EditInternal
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid ClientId { get; set; }
            public string ResponsiblePersonContact { get; set; }
            public string BankManagerContact { get; set; }
            public string ManagerNotes { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            private readonly ILogger<EditInternal> _logger;

            public Handler(
                DataContext context,
                IUserAccessor userAccessor,
                ILogger<EditInternal> logger
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
                // Доступ: Admin или Senior_Accountant
                if (!_userAccessor.IsAdmin() && !_userAccessor.IsSeniorAccountant())
                {
                    _logger.LogWarning(
                        "Попытка изменения внутренних данных без прав Level 2. ClientId: {Id}",
                        request.ClientId
                    );
                    return Result<Unit>.Failure("Недостаточно прав (требуется уровень Senior).");
                }

                var client = await _context
                    .Clients.Include(x => x.Internal)
                    .FirstOrDefaultAsync(x => x.Id == request.ClientId, cancellationToken);

                if (client == null)
                    return Result<Unit>.Failure("Клиент не найден.");

                // Инициализируем Internal, если запись еще не создана
                client.Internal ??= new ClientInternal { ClientId = client.Id };

                // Обновляем поля Level 2
                client.Internal.ResponsiblePersonContact = request.ResponsiblePersonContact;
                client.Internal.BankManagerContact = request.BankManagerContact;
                client.Internal.ManagerNotes = request.ManagerNotes;

                _logger.LogInformation(
                    "Внутренние данные клиента {Id} обновлены пользователем {User}",
                    client.Id,
                    _userAccessor.GetUserName()
                );

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                // Возвращаем успех, даже если данные не изменились (SaveChangesAsync вернет 0)
                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
