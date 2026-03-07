using Application.Common.Interfaces;
using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Storage;

namespace Application.Transactions
{
    public class GetList
    {
        public class Query : IRequest<Result<PagedList<TransactionDto>>>
        {
            public TransactionParams Params { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<PagedList<TransactionDto>>>
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

            public async Task<Result<PagedList<TransactionDto>>> Handle(
                Query request,
                CancellationToken cancellationToken
            )
            {
                var userName = _userAccessor.GetUserName();
                _logger.LogInformation(
                    "User {User} is requesting transactions with parameters: {@Params}",
                    userName,
                    request.Params
                );

                try
                {
                    // 1. Base Query with Security: only transactions of clients responsible by current user
                    var query = _context
                        .Transactions.AsNoTracking()
                        .Where(t => t.Client.ResponsiblePersonContact.Contains(userName));

                    // 2. Entity Level Filters (Handling Guid? to int conversion if necessary)
                    if (request.Params.ClientId.HasValue)
                    {
                        // Safely parsing or casting depending on how your Params are defined
                        if (int.TryParse(request.Params.ClientId.ToString(), out int clientId))
                        {
                            query = query.Where(x => x.ClientId == request.Params.ClientId.Value);
                        }
                    }

                    if (request.Params.ServiceId.HasValue)
                    {
                        if (int.TryParse(request.Params.ServiceId.ToString(), out int serviceId))
                        {
                            query = query.Where(x => x.ServiceId == request.Params.ServiceId);
                        }
                    }

                    if (request.Params.StartDate.HasValue)
                        query = query.Where(x => x.Date >= request.Params.StartDate.Value);

                    if (request.Params.EndDate.HasValue)
                        query = query.Where(x => x.Date <= request.Params.EndDate.Value);

                    if (!string.IsNullOrEmpty(request.Params.Status))
                        query = query.Where(x => x.Status == request.Params.Status);

                    // 3. Project to DTO (Automapper handles related data joins)
                    var dtoQuery = query.ProjectTo<TransactionDto>(_mapper.ConfigurationProvider);

                    // 4. Global Search via Expression Trees
                    if (!string.IsNullOrWhiteSpace(request.Params.Search))
                    {
                        var searchPredicate = _searchBuilder.BuildSearchExpression<TransactionDto>(
                            request.Params.Search
                        );
                        dtoQuery = dtoQuery.Where(searchPredicate);
                    }

                    // 5. Dynamic Sorting
                    var sortField = request.Params.SortField?.ToLower() ?? "date";
                    dtoQuery = sortField switch
                    {
                        "date" => request.Params.Order == "asc"
                            ? dtoQuery.OrderBy(t => t.Date)
                            : dtoQuery.OrderByDescending(t => t.Date),
                        "clientname" => request.Params.Order == "asc"
                            ? dtoQuery.OrderBy(t => t.ClientName)
                            : dtoQuery.OrderByDescending(t => t.ClientName),
                        "amount" => request.Params.Order == "asc"
                            ? dtoQuery.OrderBy(t => t.ExtraServiceAmount)
                            : dtoQuery.OrderByDescending(t => t.ExtraServiceAmount),
                        _ => dtoQuery.OrderByDescending(t => t.Date),
                    };

                    // 6. Pagination and Result
                    var pagedList = await PagedList<TransactionDto>.CreateAsync(
                        dtoQuery,
                        request.Params.PageNumber,
                        request.Params.PageSize
                    );

                    return Result<PagedList<TransactionDto>>.Success(pagedList);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error occurred while fetching transactions for user {User}",
                        userName
                    );
                    return Result<PagedList<TransactionDto>>.Failure(
                        "An error occurred while loading the transaction journal."
                    );
                }
            }
        }
    }
}
