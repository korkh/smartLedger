using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Constants;
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
                    "Пользователь {User} запрашивает список клиентов: {@Params}",
                    userName,
                    request.Params
                );

                try
                {
                    // ---------------------------------------------------------
                    // 1. Базовый запрос
                    // ---------------------------------------------------------
                    var query = _context
                        .Clients.Include(x => x.CurrentTariff)
                        .AsNoTracking()
                        .AsQueryable();

                    // ---------------------------------------------------------
                    // 2. Ограничение доступа (не Admin → только свои клиенты)
                    // ---------------------------------------------------------
                    if (!_userAccessor.IsAdmin())
                    {
                        query = query.Where(x =>
                            x.Internal != null
                            && x.Internal.ResponsiblePersonContact != null
                            && x.Internal.ResponsiblePersonContact.Contains(userName)
                        );
                    }

                    // ---------------------------------------------------------
                    // 3. Фильтры
                    // ---------------------------------------------------------
                    if (!string.IsNullOrEmpty(request.Params.BinIin))
                        query = query.Where(x => x.BinIin.Contains(request.Params.BinIin));

                    if (!string.IsNullOrEmpty(request.Params.TaxRegime))
                    {
                        if (Enum.TryParse<TaxRegime>(request.Params.TaxRegime, out var regime))
                            query = query.Where(x => x.TaxRegime == regime);
                    }

                    if (!string.IsNullOrEmpty(request.Params.NdsStatus))
                        query = query.Where(x => x.NdsStatus == request.Params.NdsStatus);

                    // --- Фильтр по ЭЦП ---
                    if (request.Params.EcpWarningOnly)
                    {
                        var today = DateTime.UtcNow;
                        var threshold = today.AddDays(request.Params.EcpWarningDays);

                        query = query.Where(x =>
                            x.EcpExpiryDate.HasValue
                            && x.EcpExpiryDate.Value >= today
                            && x.EcpExpiryDate.Value <= threshold
                        );
                    }

                    // ---------------------------------------------------------
                    // 4. Проекция в DTO
                    // ---------------------------------------------------------
                    var dtoQuery = query.ProjectTo<ClientDto>(_mapper.ConfigurationProvider);

                    // ---------------------------------------------------------
                    // 5. Глобальный поиск
                    // ---------------------------------------------------------
                    if (!string.IsNullOrWhiteSpace(request.Params.Search))
                    {
                        var predicate = _searchBuilder.BuildSearchExpression<ClientDto>(
                            request.Params.Search
                        );
                        dtoQuery = dtoQuery.Where(predicate);
                    }

                    // ---------------------------------------------------------
                    // 6. Сортировка
                    // ---------------------------------------------------------
                    dtoQuery = request.Params.SortField?.ToLower() switch
                    {
                        "firstname" => request.Params.Order == "asc"
                            ? dtoQuery.OrderBy(x => x.FirstName)
                            : dtoQuery.OrderByDescending(x => x.FirstName),

                        "lastname" => request.Params.Order == "asc"
                            ? dtoQuery.OrderBy(x => x.LastName)
                            : dtoQuery.OrderByDescending(x => x.LastName),

                        "bin" => request.Params.Order == "asc"
                            ? dtoQuery.OrderBy(x => x.BinIin)
                            : dtoQuery.OrderByDescending(x => x.BinIin),

                        "ecpdate" => request.Params.Order == "asc"
                            ? dtoQuery.OrderBy(x => x.EcpExpiryDate)
                            : dtoQuery.OrderByDescending(x => x.EcpExpiryDate),

                        _ => dtoQuery.OrderBy(x => x.FirstName),
                    };

                    // ---------------------------------------------------------
                    // 7. Пагинация
                    // ---------------------------------------------------------
                    var paged = await PagedList<ClientDto>.CreateAsync(
                        dtoQuery,
                        request.Params.PageNumber,
                        request.Params.PageSize
                    );

                    // ---------------------------------------------------------
                    // 8. DaysUntilEcpExpires (без foreach)
                    // ---------------------------------------------------------
                    foreach (var item in paged)
                    {
                        if (item.EcpExpiryDate.HasValue)
                            item.DaysUntilEcpExpires = (
                                item.EcpExpiryDate.Value - DateTime.UtcNow
                            ).Days;
                    }

                    return Result<PagedList<ClientDto>>.Success(paged);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Ошибка при загрузке списка клиентов пользователем {User}",
                        userName
                    );
                    return Result<PagedList<ClientDto>>.Failure("Ошибка при загрузке данных.");
                }
            }
        }
    }
}
