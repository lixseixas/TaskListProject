using System.ComponentModel.DataAnnotations;

namespace TaskReportApi.Models;

/// <summary>Represents a movement returned by the API.</summary>
public sealed record AccountMovementModel
{
    public Guid Id { get; init; }

    [Required]
    public Guid UserId { get; init; }

    [Required]
    public decimal Amount { get; init; }

    [Required]
    public string Type { get; init; } = string.Empty;

    [Required]
    public DateTime Date { get; init; }

    public string? Description { get; init; }
}
