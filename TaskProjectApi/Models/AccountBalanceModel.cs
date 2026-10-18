namespace TaskReportApi.Models;

/// <summary>Represents the calculated balance of a user.</summary>
public sealed record AccountBalanceModel
{
    public decimal Balance { get; init; }
    public Guid UserId { get; init; }
}
