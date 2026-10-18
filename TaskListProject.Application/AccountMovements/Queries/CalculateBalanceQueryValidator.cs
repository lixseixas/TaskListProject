using FluentValidation;

namespace TaskListProject.Application.AccountMovements.Queries;

public class CalculateBalanceQueryValidator : AbstractValidator<CalculateBalanceQuery>
{
    public CalculateBalanceQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");
    }
}
