using Application.Common.Interfaces; // Для IUserAccessor
using Application.Core;
using Application.Services;
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
                var currentUserName = _userAccessor.GetUserName();

                _logger.LogInformation(
                    "Пользователь {User} запрашивает Dashboard клиента {Id}",
                    currentUserName,
                    request.Id
                );

                try
                {
                    // Вызываем сервис, передавая имя пользователя для проверки прав доступа
                    var data = await _dashboardService.GetDashboardDataAsync(
                        request.Id,
                        request.Year,
                        request.Month,
                        currentUserName // Передаем владельца
                    );

                    if (data == null)
                    {
                        _logger.LogWarning(
                            "Доступ запрещен или клиент {Id} не найден для {User}",
                            request.Id,
                            currentUserName
                        );
                        return null;
                    }

                    return Result<ClientDashboardDto>.Success(data);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка Dashboard для клиента {Id}", request.Id);
                    return Result<ClientDashboardDto>.Failure("Ошибка при расчете лимитов.");
                }
            }
        }
    }
}
