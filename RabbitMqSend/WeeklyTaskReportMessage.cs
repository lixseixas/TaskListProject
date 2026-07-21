namespace RabbitMqSend;

public sealed class WeeklyTaskReportMessage
{
    public DateTime WeekStartDate { get; init; }
    public DateTime WeekEndDate { get; init; }
    public int WeekNumber { get; init; }
    public int Year { get; init; }
    public int TotalTasks { get; init; }
    public int CompletedTasks { get; init; }
    public int PendingTasks { get; init; }
    public double CompletionPercentage { get; init; }
}
