using System.ComponentModel.DataAnnotations;

namespace TaskReportApi.Models;

/// <summary>
/// Represents a task in the system
/// </summary>
public sealed record TaskModel
{
    public Guid Id { get; init; }

    [Required]
    public string Title { get; init; } = string.Empty;

    [Required]
    public string Description { get; init; } = string.Empty;

    [Required]
    public DateTime Date { get; init; }

    [Required]
    public DateTime InitialHour { get; init; }

    [Required]
    public DateTime FinalHour { get; init; }

    [Required]
    public int Priority { get; init; }

    [Required]
    public bool Ended { get; init; }
}
