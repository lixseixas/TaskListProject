using MediatR;

namespace TaskListProject.Application.AccountMovements.Queries;

public sealed record CalculateBalanceQuery : IRequest<AccountBalanceResult>
{
    public Guid UserId { get; init; }
}
