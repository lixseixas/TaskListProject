using FluentValidation;
using TaskReportApi.CQRS.AccountMovements.Commands;

namespace TaskReportApi.CQRS.AccountMovements.Commands;

public class CreateMovementCommandValidator : AbstractValidator<CreateMovementCommand>
{
    public CreateMovementCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.Amount)
            .NotEmpty().WithMessage("Amount is required")
            .GreaterThan(0).WithMessage("Amount must be greater than 0");       

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required");
    }
}
