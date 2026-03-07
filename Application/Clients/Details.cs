using Application.Common.Interfaces;
using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Added for logging
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
            private readonly ILogger<Details> _logger; // Logger field

            public Handler(
                DataContext context,
                IMapper mapper,
                IUserAccessor userAccessor,
                ILogger<Details> logger
            ) // Injected logger
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
                _logger.LogInformation("Запрос данных для клиента с ID: {Id}", request.Id);

                try
                {
                    // ProjectTo автоматически подгружает связанные данные (Transactions, Tariff)
                    var client = await _context
                        .Clients.ProjectTo<ClientDto>(
                            _mapper.ConfigurationProvider,
                            new { currentUsername = _userAccessor.GetUserName() }
                        )
                        .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                    if (client == null)
                    {
                        _logger.LogWarning("Клиент с ID {Id} не найден в базе данных", request.Id);
                        return null;
                    }

                    return Result<ClientDto>.Success(client);
                }
                catch (Exception ex)
                {
                    // Логируем исключение с контекстом и русским описанием
                    _logger.LogError(
                        ex,
                        "Критическая ошибка при получении данных о клиенте с ID: {Id}",
                        request.Id
                    );

                    return Result<ClientDto>.Failure(
                        "Произошла ошибка при получении данных о клиенте."
                    );
                }
            }
        }
    }
}
