using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Storage;

namespace Application.Transactions
{
    public class Details
    {
        public class Query : IRequest<Result<TransactionDto>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<TransactionDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly ILogger<Details> _logger;

            public Handler(DataContext context, IMapper mapper, ILogger<Details> logger)
            {
                _context = context;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<Result<TransactionDto>> Handle(
                Query request,
                CancellationToken cancellationToken
            )
            {
                _logger.LogInformation("Запрос деталей транзакции. ID: {Id}", request.Id);

                try
                {
                    // ВАЖНО: Сначала фильтруем по сущности (где Id — это int),
                    // а затем проецируем в DTO через ProjectTo.
                    var transaction = await _context
                        .Transactions.Where(x => x.Id == request.Id) // Сравнение int == int
                        .ProjectTo<TransactionDto>(_mapper.ConfigurationProvider)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (transaction == null)
                    {
                        _logger.LogWarning("Транзакция с ID {Id} не найдена в системе", request.Id);
                        return null;
                    }

                    _logger.LogInformation("Данные транзакции {Id} успешно получены", request.Id);
                    return Result<TransactionDto>.Success(transaction);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Ошибка при получении деталей транзакции {Id}",
                        request.Id
                    );
                    return Result<TransactionDto>.Failure(
                        "Произошла системная ошибка при загрузке деталей транзакции."
                    );
                }
            }
        }
    }
}
