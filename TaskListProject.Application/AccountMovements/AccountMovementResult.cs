namespace TaskListProject.Application.AccountMovements;

public sealed record AccountMovementResult
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public string Type { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public string? Description { get; init; }
}
