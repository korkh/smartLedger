using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Interfaces;
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
                try
                {
                    var client = await _context
                        .Clients.ProjectTo<ClientDto>(
                            _mapper.ConfigurationProvider,
                            new { currentUsername = _userAccessor.GetUserName() }
                        )
                        .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                    if (client == null)
                        return Result<ClientDto>.Failure("Клиент не найден.");

                    return Result<ClientDto>.Success(client);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при получении клиента");
                    return Result<ClientDto>.Failure("Ошибка при получении данных клиента.");
                }
            }
        }
    }
}
