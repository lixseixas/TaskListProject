namespace TaskListProject.Application.AccountMovements;

public sealed record AccountMovementsResult
{
    public List<AccountMovementResult> Movements { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}
