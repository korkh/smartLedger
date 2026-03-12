using Application.Transactions;
using FluentValidation;

namespace Application.Common.Helpers.Validators
{
    public class TransactionValidator : AbstractValidator<TransactionDto>
    {
        public TransactionValidator()
        {
            // 1. Базовая проверка даты и услуги
            RuleFor(x => x.Date).NotEmpty().WithMessage("Дата транзакции обязательна.");

            RuleFor(x => x.ServiceTypeName).NotEmpty().WithMessage("Название услуги обязательно.");

            // 2. Валидация количества операций
            RuleFor(x => x.OperationsCount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Операции не могут быть отрицательными.");

            // 3. Валидация временных интервалов (вместо DurationMinutes)
            RuleFor(x => x.ActualTimeMinutes)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Актуальное время не может быть отрицательным.");

            RuleFor(x => x.BillableTimeMinutes)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Время счета не может быть отрицательным.");

            RuleFor(x => x.CommunicationTimeMinutes)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Время связи не может быть отрицательным.");

            // 4. Валидация суммы дополнительных услуг (если есть)
            RuleFor(x => x.ExtraServiceAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Дополнительные услуги не могут быть отрицательными.");
        }
    }
}
