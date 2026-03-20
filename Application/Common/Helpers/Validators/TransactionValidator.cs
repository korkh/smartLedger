using Application.Transactions;
using FluentValidation;

namespace Application.Common.Helpers.Validators
{
    public class TransactionValidator : AbstractValidator<TransactionDto>
    {
        public TransactionValidator()
        {
            // 1. Дата
            RuleFor(x => x.Date).NotEmpty().WithMessage("Дата транзакции обязательна.");

            // 2. Название услуги
            RuleFor(x => x.ServiceCategoryName)
                .NotEmpty()
                .WithMessage("Название услуги обязательно.");

            // 3. Количество операций
            RuleFor(x => x.OperationsCount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Количество операций не может быть отрицательным.");

            // 4. Время
            RuleFor(x => x.ActualTimeMinutes)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Актуальное время не может быть отрицательным.");

            RuleFor(x => x.BillableTimeMinutes)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Время счета не может быть отрицательным.");

            RuleFor(x => x.CommunicationTimeMinutes)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Время связи не может быть отрицательным.");

            RuleFor(x => x.TeamWorkMinutes)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Время командной работы не может быть отрицательным.");

            RuleFor(x => x.ForceMajeureMinutes)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Форс-мажорное время не может быть отрицательным.");

            // 5. Доп. услуги
            RuleFor(x => x.ExtraServiceAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Сумма дополнительных услуг не может быть отрицательной.");

            RuleFor(x => x.ExtraServiceCount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Количество дополнительных услуг не может быть отрицательным.");

            // 6. НДС
            RuleFor(x => x.NdsBaseAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Сумма НДС не может быть отрицательной.");
        }
    }
}
