using Application.Common.Helpers.Validators;
using Application.Core;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Storage;

namespace Application.Transactions
{
    public class Edit
    {
        public class Command : IRequest<Result<Unit>>
        {
            public TransactionDto Transaction { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Transaction).SetValidator(new TransactionValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly ILogger<Edit> _logger;

            public Handler(DataContext context, IMapper mapper, ILogger<Edit> logger)
            {
                _context = context;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
            {
                try
                {
                    var entity = await _context.Transactions.FirstOrDefaultAsync(
                        x => x.Id == request.Transaction.Id,
                        ct
                    );

                    if (entity == null)
                        return Result<Unit>.Failure("Транзакция не найдена.");

                    // Маппим DTO → Entity
                    _mapper.Map(request.Transaction, entity);

                    var saved = await _context.SaveChangesAsync(ct) > 0;

                    if (!saved)
                        return Result<Unit>.Failure("Изменения не были сохранены.");

                    return Result<Unit>.Success(Unit.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обновлении транзакции");
                    return Result<Unit>.Failure("Системная ошибка при обновлении транзакции.");
                }
            }
        }
    }
}
