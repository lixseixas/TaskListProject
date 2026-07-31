using FluentValidation;
using TaskProject.CQRS.Tasks.Queries;

namespace TaskProject.CQRS.Tasks.Validators;

public class GetSummarizedTasksQueryValidator : AbstractValidator<GetSummarizedTasksQuery>
{
    public GetSummarizedTasksQueryValidator()
    {
        RuleFor(x => x.InitialDate)
            .NotEmpty().WithMessage("Initial date is required")
            .LessThanOrEqualTo(x => x.FinalDate).WithMessage("Initial date must be before or equal to final date");

        RuleFor(x => x.FinalDate)
            .NotEmpty().WithMessage("Final date is required");
    }
}
