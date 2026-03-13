using Application.Core;
using MediatR;
using Storage;

namespace Application.Transactions
{
    public class SoftDelete
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context) => _context = context;

            public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
            {
                var transaction = await _context.Transactions.FindAsync(request.Id);
                if (transaction == null)
                    return null;

                transaction.IsDeleted = true;
                transaction.DeletedAt = DateTime.UtcNow;

                var result = await _context.SaveChangesAsync(ct) > 0;
                return result
                    ? Result<Unit>.Success(Unit.Value)
                    : Result<Unit>.Failure("Ошибка при архивации");
            }
        }
    }
}
