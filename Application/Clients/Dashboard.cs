using Application.Core;
using Application.Services;
using Domain.Interfaces; // Для IUserAccessor
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

        public class Handler(
            ClientAppService dashboardService,
            ILogger<Dashboard> logger,
            IUserAccessor userAccessor
        ) : IRequestHandler<Query, Result<ClientDashboardDto>>
        {
            private readonly ClientAppService _dashboardService = dashboardService;
            private readonly ILogger<Dashboard> _logger = logger;
            private readonly IUserAccessor _userAccessor = userAccessor;

            public async Task<Result<ClientDashboardDto>> Handle(
                Query request,
                CancellationToken cancellationToken
            )
            {
                var currentUserName = _userAccessor.GetUserName();
                // Проверяем, есть ли у пользователя роль Admin в токене
                var isAdmin = _userAccessor.IsAdmin();

                _logger.LogInformation(
                    "Пользователь {User} (Admin: {IsAdmin}) запрашивает Dashboard клиента {Id}",
                    currentUserName,
                    isAdmin,
                    request.Id
                );

                try
                {
                    // Вызываем сервис, передавая имя пользователя для проверки прав доступа
                    var data = await _dashboardService.GetDashboardDataAsync(
                        request.Id,
                        request.Year,
                        request.Month,
                        currentUserName,
                        isAdmin
                    );

                    if (data == null)
                    {
                        _logger.LogWarning(
                            "Доступ запрещен или клиент {Id} не найден для {User}",
                            request.Id,
                            currentUserName
                        );
                        // Возвращаем пустой успех, чтобы HandleResult выдал NotFound
                        return Result<ClientDashboardDto>.Success(null);
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
