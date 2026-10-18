namespace TaskReportApi.Models;

/// <summary>Payload for creating an account movement.</summary>
public sealed record CreateMovementRequest
{
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public string Type { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public string? Description { get; init; }
}
