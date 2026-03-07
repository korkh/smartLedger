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
    public class GetList
    {
        public class Query : IRequest<Result<PagedList<ClientDto>>>
        {
            public ClientParams Params { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<PagedList<ClientDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly ISearchExpressionBuilder _searchBuilder;
            private readonly IUserAccessor _userAccessor;
            private readonly ILogger<GetList> _logger; // Logger field

            public Handler(
                DataContext context,
                IMapper mapper,
                ISearchExpressionBuilder searchBuilder,
                IUserAccessor userAccessor,
                ILogger<GetList> logger
            ) // Injected logger
            {
                _context = context;
                _mapper = mapper;
                _searchBuilder = searchBuilder;
                _userAccessor = userAccessor;
                _logger = logger;
            }

            public async Task<Result<PagedList<ClientDto>>> Handle(
                Query request,
                CancellationToken cancellationToken
            )
            {
                var userName = _userAccessor.GetUserName();
                _logger.LogInformation(
                    "Пользователь {User} запрашивает список клиентов с параметрами: {@Params}",
                    userName,
                    request.Params
                );

                try
                {
                    // 1. Инициализация запроса (Entity уровень)
                    var query = _context.Clients.AsNoTracking();

                    // 2. Базовая фильтрация и безопасность (Entity уровень - максимально быстро)
                    query = query.Where(x => x.ResponsiblePersonContact.Contains(userName));

                    if (!string.IsNullOrEmpty(request.Params.BinIin))
                        query = query.Where(x => x.BinIin.Contains(request.Params.BinIin));

                    if (!string.IsNullOrEmpty(request.Params.TaxRegime))
                        query = query.Where(x => x.TaxRegime == request.Params.TaxRegime);

                    if (!string.IsNullOrEmpty(request.Params.NdsStatus))
                        query = query.Where(x => x.NdsStatus == request.Params.NdsStatus);

                    // 3. Проекция в DTO
                    var dtoQuery = query.ProjectTo<ClientDto>(_mapper.ConfigurationProvider);

                    // 4. Глобальный поиск (уже по DTO полям)
                    // Важно: ISearchExpressionBuilder теперь работает внутри IQueryable
                    if (!string.IsNullOrWhiteSpace(request.Params.Search))
                    {
                        _logger.LogDebug(
                            "Применение глобального поиска: {SearchText}",
                            request.Params.Search
                        );

                        var searchPredicate = _searchBuilder.BuildSearchExpression<ClientDto>(
                            request.Params.Search
                        );

                        dtoQuery = dtoQuery.Where(searchPredicate);
                    }

                    // 5. Динамическая сортировка (уже по DTO)
                    dtoQuery = (request.Params.SortField?.ToLower()) switch
                    {
                        "firstname" => request.Params.Order == "asc"
                            ? dtoQuery.OrderBy(e => e.FirstName)
                            : dtoQuery.OrderByDescending(e => e.FirstName),
                        "lastname" => request.Params.Order == "asc"
                            ? dtoQuery.OrderBy(e => e.LastName)
                            : dtoQuery.OrderByDescending(e => e.LastName),
                        "bin" => request.Params.Order == "asc"
                            ? dtoQuery.OrderBy(e => e.BinIin)
                            : dtoQuery.OrderByDescending(e => e.BinIin),
                        _ => dtoQuery.OrderBy(e => e.FirstName), // Дефолтная сортировка
                    };

                    // 6. Пагинация и выполнение запроса в БД
                    var pagedList = await PagedList<ClientDto>.CreateAsync(
                        dtoQuery,
                        request.Params.PageNumber,
                        request.Params.PageSize
                    );

                    _logger.LogInformation(
                        "Успешно получено {Count} клиентов для пользователя {User}",
                        pagedList.Count,
                        userName
                    );

                    return Result<PagedList<ClientDto>>.Success(pagedList);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Критическая ошибка при получении списка клиентов для {User}",
                        userName
                    );
                    return Result<PagedList<ClientDto>>.Failure(
                        "Произошла ошибка при загрузке данных."
                    );
                }
            }
        }
    }
}
