using Application.Common.Helpers.Validators;
using Application.Core;
using AutoMapper;
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
            private readonly ILogger<Edit> _logger; // Добавили типизацию

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
                var clientId = request.Client.Id;
                // 2. Находим существующего клиента
                var client = await _context
                    .Clients.Include(x => x.CurrentTariff)
                    .FirstOrDefaultAsync(x => x.Id == clientId, cancellationToken);

                if (client == null)
                {
                    _logger.LogWarning("Клиент с ID {Id} не был найден", clientId);
                    return null;
                }
                // 2. РУЧНАЯ ЗАЩИТА (на случай, если маппер пропустит null)
                // Если в запросе пришел null для полей 3-го уровня,
                // мы подставляем в запрос значения, которые уже лежат в базе.
                if (request.Client.StrategicNotes == null)
                    request.Client.StrategicNotes = client.StrategicNotes;

                if (request.Client.PersonalInfo == null)
                    request.Client.PersonalInfo = client.PersonalInfo;

                // 3. Маппинг (Обновляем только поля из DTO)
                _mapper.Map(request.Client, client);

                // 4. Сохранение с обработкой ошибок
                try
                {
                    // Если ничего не изменилось (например, Level 2 просто открыл и закрыл карточку),
                    // SaveChangesAsync вернет 0.
                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                    if (!result)
                        return Result<Unit>.Failure(
                            "Изменения не были сохранены (возможно, данные идентичны)."
                        );

                    return Result<Unit>.Success(Unit.Value);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Конфликт параллелизма для клиента {Id}", clientId);
                    return Result<Unit>.Failure(
                        "Данные были изменены другим пользователем. Обновите страницу."
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обновлении клиента {ClientId}", clientId);
                    return Result<Unit>.Failure("Системная ошибка при сохранении данных.");
                }
            }
        }
    }
}
