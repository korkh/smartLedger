using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
            private readonly ILogger<GetList> _logger;

            public Handler(
                DataContext context,
                IMapper mapper,
                ISearchExpressionBuilder searchBuilder,
                IUserAccessor userAccessor,
                ILogger<GetList> logger
            )
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
                    // 1. Инициализация запроса
                    var query = _context
                        .Clients.Include(x => x.CurrentTariff)
                        .Include(x => x.Transactions)
                        .AsNoTracking();

                    // 2. Базовая фильтрация и безопасность
                    if (userName.ToLower() != "admin")
                    {
                        query = query.Where(x => x.ResponsiblePersonContact.Contains(userName));
                    }

                    if (!string.IsNullOrEmpty(request.Params.BinIin))
                        query = query.Where(x => x.BinIin.Contains(request.Params.BinIin));

                    if (!string.IsNullOrEmpty(request.Params.TaxRegime))
                        query = query.Where(x => x.TaxRegime == request.Params.TaxRegime);

                    if (!string.IsNullOrEmpty(request.Params.NdsStatus))
                        query = query.Where(x => x.NdsStatus == request.Params.NdsStatus);

                    // --- НОВОЕ: Фильтрация по сроку ЭЦП ---
                    if (request.Params.EcpWarningOnly)
                    {
                        var today = DateTime.UtcNow;
                        var warningThreshold = today.AddDays(request.Params.EcpWarningDays);

                        // Показываем только тех, у кого срок в диапазоне [сегодня; сегодня + N дней]
                        query = query.Where(x =>
                            x.EcpExpiryDate.HasValue
                            && x.EcpExpiryDate.Value <= warningThreshold
                            && x.EcpExpiryDate.Value >= today
                        );

                        _logger.LogDebug(
                            "Применен фильтр критических ЭЦП: порог {Days} дней",
                            request.Params.EcpWarningDays
                        );
                    }

                    // 3. Проекция в DTO
                    var dtoQuery = query.ProjectTo<ClientDto>(_mapper.ConfigurationProvider);

                    // 4. Глобальный поиск (по текстовым полям DTO)
                    if (!string.IsNullOrWhiteSpace(request.Params.Search))
                    {
                        var searchPredicate = _searchBuilder.BuildSearchExpression<ClientDto>(
                            request.Params.Search
                        );
                        dtoQuery = dtoQuery.Where(searchPredicate);
                    }

                    // 5. Динамическая сортировка
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
                        "ecpdate" => request.Params.Order == "asc" // Добавили сортировку по дате ЭЦП
                            ? dtoQuery.OrderBy(e => e.EcpExpiryDate)
                            : dtoQuery.OrderByDescending(e => e.EcpExpiryDate),
                        _ => dtoQuery.OrderBy(e => e.FirstName),
                    };

                    // 6. Выполнение
                    var pagedList = await PagedList<ClientDto>.CreateAsync(
                        dtoQuery,
                        request.Params.PageNumber,
                        request.Params.PageSize
                    );

                    foreach (var item in pagedList)
                    {
                        if (item.EcpExpiryDate.HasValue)
                        {
                            item.DaysUntilEcpExpires = (
                                item.EcpExpiryDate.Value - DateTime.UtcNow
                            ).Days;
                        }
                    }

                    return Result<PagedList<ClientDto>>.Success(pagedList);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Критическая ошибка списка клиентов для {User}", userName);
                    return Result<PagedList<ClientDto>>.Failure("Ошибка при загрузке данных.");
                }
            }
        }
    }
}
