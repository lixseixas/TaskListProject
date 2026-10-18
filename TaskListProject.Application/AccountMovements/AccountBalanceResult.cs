namespace TaskListProject.Application.AccountMovements;

public sealed record AccountBalanceResult
{
    public decimal Balance { get; init; }
    public Guid UserId { get; init; }
}
