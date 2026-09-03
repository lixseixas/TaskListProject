using FluentValidation;
using TaskReportApi.CQRS.AccountMovements.Queries;

namespace TaskReportApi.CQRS.AccountMovements.Queries;

public class CalculateBalanceQueryValidator : AbstractValidator<CalculateBalanceQuery>
{
    public CalculateBalanceQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");
    }
}
