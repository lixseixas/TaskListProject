using System;

namespace TaskProject.Models;

/// <summary>
/// Represents a weekly task report entry showing task counts per week
/// </summary>
public record WeeklyTaskReportModel
{
    public Guid Id { get; set; }

    /// <summary>
    /// The start date of the week
    /// </summary>
    public DateTime WeekStartDate { get; init; }

    /// <summary>
    /// The end date of the week
    /// </summary>
    public DateTime WeekEndDate { get; init; }

    /// <summary>
    /// Week number in the year
    /// </summary>
    public int WeekNumber { get; init; }

    /// <summary>
    /// Year of the week
    /// </summary>
    public int Year { get; init; }

    /// <summary>
    /// Total number of tasks scheduled for this week
    /// </summary>
    public int TotalTasks { get; init; }

    /// <summary>
    /// Number of completed tasks
    /// </summary>
    public int CompletedTasks { get; init; }

    /// <summary>
    /// Number of pending tasks
    /// </summary>
    public int PendingTasks { get; init; }

    /// <summary>
    /// Percentage of completed tasks
    /// </summary>
    public double CompletionPercentage { get; init; }
}
