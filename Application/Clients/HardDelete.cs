using Application.Core;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Storage;

namespace Application.Clients
{
    public class HardDelete
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
            {
                if (!_userAccessor.IsAdmin())
                    return Result<Unit>.Failure(
                        "Критическая операция: Только администратор может физически удалять записи."
                    );

                // .IgnoreQueryFilters() позволяет найти клиента, даже если он уже в "корзине"
                var affected = await _context
                    .Clients.IgnoreQueryFilters()
                    .Where(x => x.Id == request.Id)
                    .ExecuteDeleteAsync(ct);

                return affected > 0
                    ? Result<Unit>.Success(Unit.Value)
                    : Result<Unit>.Failure("Клиент не найден в базе данных.");
            }
        }
    }
}
