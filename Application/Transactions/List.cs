using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Storage;

namespace Application.Transactions
{
    public class List
    {
        public class Query : IRequest<Result<PagedList<TransactionDto>>>
        {
            public TransactionParams Params { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<PagedList<TransactionDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly ILogger<List> _logger;

            public Handler(DataContext context, IMapper mapper, ILogger<List> logger)
            {
                _context = context;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<Result<PagedList<TransactionDto>>> Handle(
                Query request,
                CancellationToken cancellationToken
            )
            {
                _logger.LogInformation(
                    "Запрос списка транзакций с параметрами: {@Params}",
                    request.Params
                );

                try
                {
                    // 1. Начинаем построение запроса (AsNoTracking для производительности)
                    var query = _context.Transactions.AsNoTracking();

                    // 2. Фильтрация по Клиенту
                    if (request.Params.ClientId.HasValue)
                    {
                        // Приводим Guid к int, если в БД Id целочисленный
                        if (int.TryParse(request.Params.ClientId.ToString(), out int clientId))
                        {
                            query = query.Where(x => x.ClientId == clientId);
                        }
                    }

                    // 3. Фильтрация по Услуге
                    if (request.Params.ServiceId.HasValue)
                    {
                        if (int.TryParse(request.Params.ServiceId.ToString(), out int serviceId))
                        {
                            query = query.Where(x => x.ServiceId == serviceId);
                        }
                    }

                    // 4. Фильтрация по периоду (Критично для налогов!)
                    if (request.Params.StartDate.HasValue)
                        query = query.Where(x => x.Date >= request.Params.StartDate.Value);

                    if (request.Params.EndDate.HasValue)
                        query = query.Where(x => x.Date <= request.Params.EndDate.Value);

                    // 5. Фильтрация по статусу
                    if (!string.IsNullOrEmpty(request.Params.Status))
                        query = query.Where(x => x.Status == request.Params.Status);

                    // 6. Проекция в DTO (AutoMapper подтянет ClientName и ServiceName через Joins)
                    var dtoQuery = query.ProjectTo<TransactionDto>(_mapper.ConfigurationProvider);

                    // 7. Сортировка (по умолчанию самая свежая дата сверху)
                    dtoQuery = request.Params.SortField.ToLower() switch
                    {
                        "date" => request.Params.Order == "asc"
                            ? dtoQuery.OrderBy(t => t.Date)
                            : dtoQuery.OrderByDescending(t => t.Date),
                        "clientname" => request.Params.Order == "asc"
                            ? dtoQuery.OrderBy(t => t.ClientName)
                            : dtoQuery.OrderByDescending(t => t.ClientName),
                        _ => dtoQuery.OrderByDescending(t => t.Date),
                    };

                    // 8. Пагинация и возврат результата
                    var pagedList = await PagedList<TransactionDto>.CreateAsync(
                        dtoQuery,
                        request.Params.PageNumber,
                        request.Params.PageSize
                    );

                    _logger.LogInformation(
                        "Успешно загружено {Count} транзакций.",
                        pagedList.Count
                    );
                    return Result<PagedList<TransactionDto>>.Success(pagedList);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при получении списка транзакций.");
                    return Result<PagedList<TransactionDto>>.Failure(
                        "Произошла ошибка при загрузке журнала транзакций."
                    );
                }
            }
        }
    }
}
