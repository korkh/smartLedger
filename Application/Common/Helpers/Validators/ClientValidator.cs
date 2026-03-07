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
                .Length(1, 100)
                .WithMessage("First name must be between 1 and 100 characters.");
            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required.")
                .Length(1, 100)
                .WithMessage("Last name must be between 1 and 100 characters.");

            RuleFor(x => x.BinIin).Length(12).Matches("^[0-9]*$");

            RuleFor(x => x.Address).NotEmpty().WithMessage("Address is required.");

            RuleForEach(x => x.Transactions).SetValidator(new TransactionValidator());
        }
    }
}
