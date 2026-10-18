using MediatR;

namespace TaskListProject.Application.WeeklyTaskReports.Commands;

public sealed record InsertWeeklyTaskReportCommand : IRequest<WeeklyTaskReportResult>
{
    public Guid Id { get; init; }
    public DateTime WeekStartDate { get; init; }
    public DateTime WeekEndDate { get; init; }
    public int WeekNumber { get; init; }
    public int Year { get; init; }
    public int TotalTasks { get; init; }
    public int CompletedTasks { get; init; }
    public int PendingTasks { get; init; }
    public double CompletionPercentage { get; init; }
}
