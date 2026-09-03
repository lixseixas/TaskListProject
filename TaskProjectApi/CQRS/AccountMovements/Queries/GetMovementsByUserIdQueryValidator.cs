using FluentValidation;
using TaskReportApi.CQRS.AccountMovements.Queries;

namespace TaskReportApi.CQRS.AccountMovements.Queries;

public class GetMovementsByUserIdQueryValidator : AbstractValidator<GetMovementsByUserIdQuery>
{
    public GetMovementsByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0")
            .LessThanOrEqualTo(10).WithMessage("PageSize cannot exceed 10");
    }
}
