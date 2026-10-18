namespace TaskListProject.Application.Tasks;

public sealed record SummarizedTasksResult
{
    public DateTime Date { get; init; }
    public string Hours { get; init; } = string.Empty;
    public int TotalTasks { get; init; }
    public string AverageHours { get; init; } = string.Empty;
    public double PercentualConcludedTasks { get; init; }
}

public sealed record TaskSummaryResult(
    DateTime InitialDate, DateTime FinalDate, List<SummarizedTasksResult> ListTasksSummarized);
