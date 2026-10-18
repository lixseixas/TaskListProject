using MediatR;

namespace TaskListProject.Application.WeeklyTaskReports.Queries;

public sealed record GetWeeklyTaskReportsQuery(
    DateTime StartDate, DateTime EndDate) : IRequest<List<WeeklyTaskReportResult>>;
