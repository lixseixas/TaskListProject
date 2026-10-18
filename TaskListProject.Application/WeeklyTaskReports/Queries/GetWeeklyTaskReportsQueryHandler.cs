using Mapster;
using MediatR;
using TaskListProject.Application.Common;
using TaskListProject.Application.Data;

namespace TaskListProject.Application.WeeklyTaskReports.Queries;

public sealed class GetWeeklyTaskReportsQueryHandler(IWeeklyTaskReportRepository reports)
    : IRequestHandler<GetWeeklyTaskReportsQuery, List<WeeklyTaskReportResult>>
{
    public async Task<List<WeeklyTaskReportResult>> Handle(
        GetWeeklyTaskReportsQuery request, CancellationToken cancellationToken)
    {
        if (request.StartDate > request.EndDate)
        {
            throw new BusinessRuleException("Start date must be before or equal to end date");
        }

        if ((request.EndDate - request.StartDate).TotalDays > 1200)
        {
            throw new BusinessRuleException("Date range cannot exceed 1 year");
        }

        var result = await reports.GetAsync(
            request.StartDate.Date, request.EndDate.Date.AddDays(1).AddTicks(-1), cancellationToken);
        return result.Adapt<List<WeeklyTaskReportResult>>();
    }
}
