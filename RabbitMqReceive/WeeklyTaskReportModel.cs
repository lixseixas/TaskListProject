namespace RabbitMqReceive;

public sealed class WeeklyTaskReportModel
{
    public Guid Id { get; set; }
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public int WeekNumber { get; set; }
    public int Year { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public double CompletionPercentage { get; set; }
}
