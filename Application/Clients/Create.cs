using Application.Common.Helpers.Validators;
using Application.Core;
using Application.Transactions;
using AutoMapper;
using Domain.Entitites;
using FluentValidation;
using MediatR;
using Storage;

namespace Application.Clients
{
    public class Create
    {
        public class Command : IRequest<Result<Unit>>
        {
            public ClientDto Client { get; set; }
            public List<TransactionDto> Transactions { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Client).SetValidator(new ClientValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<Unit>> Handle(
                Command request,
                CancellationToken cancellationToken
            )
            {
                var client = _mapper.Map<Client>(request.Client);

                client.Transactions =
                    (ICollection<Transaction>)_mapper.Map<Transaction>(request.Transactions);

                _context.Clients.Add(client);
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result)
                    return Result<Unit>.Failure("Falied to create training");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
