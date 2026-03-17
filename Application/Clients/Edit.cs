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
                    var client = await _context
                        .Clients.Include(x => x.CurrentTariff)
                        .FirstOrDefaultAsync(x => x.Id == request.Client.Id, cancellationToken);

                    if (client == null)
                        return Result<Unit>.Failure("Клиент не найден.");

                    // Защита Level 3 полей
                    // Загружаем чувствительные данные
                    await _context
                        .Entry(client)
                        .Reference(c => c.Sensitive)
                        .LoadAsync(cancellationToken);

                    // Если чувствительных данных нет — создаём
                    client.Sensitive ??= new ClientSensitive();

                    // Защита Level 3 полей
                    if (request.Client.StrategicNotes == null)
                        request.Client.StrategicNotes = client.Sensitive.StrategicNotes;

                    if (request.Client.PersonalInfo == null)
                        request.Client.PersonalInfo = client.Sensitive.PersonalInfo;

                    // Маппинг DTO → Client
                    _mapper.Map(request.Client, client);

                    // Маппинг Level 3 → ClientSensitive
                    client.Sensitive.StrategicNotes = request.Client.StrategicNotes;
                    client.Sensitive.PersonalInfo = request.Client.PersonalInfo;

                    _mapper.Map(request.Client, client);

                    bool saved = await _context.SaveChangesAsync(cancellationToken) > 0;

                    if (!saved)
                        return Result<Unit>.Failure("Изменения не были сохранены.");

                    return Result<Unit>.Success(Unit.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обновлении клиента");
                    return Result<Unit>.Failure("Системная ошибка при обновлении клиента.");
                }
            }
        }
    }
}
