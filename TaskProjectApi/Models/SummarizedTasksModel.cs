using System;

namespace TaskReportApi.Models;

/// <summary>Represents task totals and hours for a date.</summary>
public sealed record SummarizedTasksModel
{
    public DateTime Date { get; init; }
    public string Hours { get; init; } = string.Empty;
    public int TotalTasks { get; init; }
    public string AverageHours { get; init; } = string.Empty;
    public double PercentualConcludedTasks { get; init; }
}
