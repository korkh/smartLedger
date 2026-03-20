using Application.Core;
using AutoMapper;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Storage;

namespace Application.Clients
{
    public class Details
    {
        public class Query : IRequest<Result<ClientDto>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<ClientDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly IUserAccessor _userAccessor;
            private readonly ILogger<Details> _logger;

            public Handler(
                DataContext context,
                IMapper mapper,
                IUserAccessor userAccessor,
                ILogger<Details> logger
            )
            {
                _context = context;
                _mapper = mapper;
                _userAccessor = userAccessor;
                _logger = logger;
            }

            public async Task<Result<ClientDto>> Handle(
                Query request,
                CancellationToken cancellationToken
            )
            {
                // Загружаем клиента со всеми уровнями
                var client = await _context
                    .Clients.Include(c => c.Internal)
                    .Include(c => c.Sensitive)
                    .Include(c => c.ClientTariffs)
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                if (client == null)
                    return Result<ClientDto>.Failure("Клиент не найден.");

                // Мапим в ClientDto. Благодаря ValueConverter, в client.Sensitive.EcpPassword
                // уже лежит расшифрованный текст.
                var dto = _mapper.Map<ClientDto>(client);

                // Логика безопасности: фильтруем DTO перед выходом из Application Layer
                if (!_userAccessor.IsAdmin())
                {
                    // 1. Clearing fields for non-admin
                    dto.EcpPassword = "********";
                    dto.EsfPassword = "********";
                    dto.BankingPasswords = "********";
                    dto.StrategicNotes = null;
                    dto.PersonalInfo = null;

                    // 2. Если это просто Junior, скрываем Level 2
                    if (!_userAccessor.IsSeniorAccountant())
                    {
                        dto.ResponsiblePersonContact = null;
                        dto.BankManagerContact = null;
                        dto.ManagerNotes = null;
                    }
                }

                // Расчет "дней до истечения ЭЦП"
                if (client.EcpExpiryDate.HasValue)
                {
                    dto.DaysUntilEcpExpires = (client.EcpExpiryDate.Value - DateTime.UtcNow).Days;
                }

                return Result<ClientDto>.Success(dto);
            }
        }
    }
}
