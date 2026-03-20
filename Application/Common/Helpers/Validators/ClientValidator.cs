using Application.Clients;
using FluentValidation;

namespace Application.Common.Helpers.Validators
{
    public class ClientValidator : AbstractValidator<ClientDto>
    {
        public ClientValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First name is required.")
                .Length(1, 100);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required.")
                .Length(1, 100);

            RuleFor(x => x.BinIin).Length(12).Matches("^[0-9]*$");

            RuleFor(x => x.Address).NotEmpty().WithMessage("Address is required.");

            // ❌ Удаляем — Transactions нет в DTO
            // RuleForEach(x => x.Transactions).SetValidator(new TransactionValidator());
        }
    }
}
