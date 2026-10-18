using MediatR;

namespace TaskListProject.Application.AccountMovements.Queries;

public sealed record GetMovementsByUserIdQuery : IRequest<AccountMovementsResult>
{
    public Guid UserId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
