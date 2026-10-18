using MediatR;

namespace TaskListProject.Application.AccountMovements.Commands;

public sealed record CreateMovementCommand : IRequest<AccountMovementResult>
{
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public string Type { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public string? Description { get; init; }
}
