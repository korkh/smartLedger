using Domain.Entitites;
using FluentValidation;

namespace Application.Common.Helpers.Validators
{
    public class TransactionValidator : AbstractValidator<Transaction>
    {
        public TransactionValidator()
        {
            RuleFor(x => x.PerformerName)
                .NotEmpty()
                .WithMessage("Требуется имя исполнителя.")
                .Length(1, 100)
                .WithMessage("Имя исполнителя должно быть от 1 до 100 символов.");
        }
    }
}
