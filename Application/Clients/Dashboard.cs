using Application.Core;
using Application.Services;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Clients
{
    public class Dashboard
    {
        public class Query : IRequest<Result<ClientDashboardDto>>
        {
            public Guid Id { get; set; }
            public int Year { get; set; } = DateTime.Now.Year;
            public int Month { get; set; } = DateTime.Now.Month;
        }

        public class Handler : IRequestHandler<Query, Result<ClientDashboardDto>>
        {
            private readonly ClientAppService _dashboardService;
            private readonly ILogger<Dashboard> _logger;
            private readonly IUserAccessor _userAccessor;

            public Handler(
                ClientAppService dashboardService,
                ILogger<Dashboard> logger,
                IUserAccessor userAccessor
            )
            {
                _dashboardService = dashboardService;
                _logger = logger;
                _userAccessor = userAccessor;
            }

            public async Task<Result<ClientDashboardDto>> Handle(
                Query request,
                CancellationToken cancellationToken
            )
            {
                var userName = _userAccessor.GetUserName();
                var isAdmin = _userAccessor.IsAdmin();
                var isSenior = _userAccessor.IsSeniorAccountant();

                _logger.LogInformation(
                    "Dashboard request for client {ClientId} by {User} (Admin: {IsAdmin}, Senior: {IsSenior})",
                    request.Id,
                    userName,
                    isAdmin,
                    isSenior
                );

                try
                {
                    // ---------------------------------------------------------
                    // Передаём уровни доступа в сервис
                    // ---------------------------------------------------------
                    var data = await _dashboardService.GetDashboardDataAsync(
                        request.Id,
                        request.Year,
                        request.Month,
                        userName,
                        isAdmin,
                        isSenior
                    );

                    if (data == null)
                    {
                        _logger.LogWarning(
                            "Dashboard access denied or client {Id} not found for user {User}",
                            request.Id,
                            userName
                        );

                        // Возвращаем Success(null), чтобы контроллер выдал 404
                        return Result<ClientDashboardDto>.Success(null);
                    }

                    return Result<ClientDashboardDto>.Success(data);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Dashboard calculation error for client {Id}", request.Id);

                    return Result<ClientDashboardDto>.Failure(
                        "Ошибка при расчёте данных дашборда."
                    );
                }
            }
        }
    }
}
