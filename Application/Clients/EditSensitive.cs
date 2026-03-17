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
                // Только Admin
                if (!_userAccessor.IsAdmin())
                    return Result<Unit>.Failure(
                        "Недостаточно прав для изменения чувствительных данных."
                    );

                var client = await _context
                    .Clients.Include(x => x.Sensitive)
                    .FirstOrDefaultAsync(x => x.Id == request.ClientId, cancellationToken);

                if (client == null)
                    return Result<Unit>.Failure("Клиент не найден.");

                client.Sensitive ??= new ClientSensitive();

                client.Sensitive.StrategicNotes = request.StrategicNotes;
                client.Sensitive.PersonalInfo = request.PersonalInfo;
                client.Sensitive.EcpPassword = request.EcpPassword;
                client.Sensitive.EsfPassword = request.EsfPassword;
                client.Sensitive.BankingPasswords = request.BankingPasswords;

                bool saved = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!saved)
                    return Result<Unit>.Failure("Изменения не были сохранены.");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
