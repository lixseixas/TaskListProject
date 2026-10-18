using Mapster;
using MediatR;
using TaskListProject.Application.Common;
using TaskListProject.Application.Data;
using TaskProject.Domain.Entities;

namespace TaskListProject.Application.WeeklyTaskReports.Commands;

public sealed class InsertWeeklyTaskReportCommandHandler(IWeeklyTaskReportRepository reports)
    : IRequestHandler<InsertWeeklyTaskReportCommand, WeeklyTaskReportResult>
{
    public async Task<WeeklyTaskReportResult> Handle(
        InsertWeeklyTaskReportCommand request, CancellationToken cancellationToken)
    {
        if (request.WeekStartDate.Date > request.WeekEndDate.Date)
        {
            throw new BusinessRuleException("Week start date must be before or equal to week end date");
        }

        if (request.TotalTasks < 0 || request.CompletedTasks < 0 || request.PendingTasks < 0)
        {
            throw new BusinessRuleException("Task counts cannot be negative");
        }

        if (request.CompletedTasks + request.PendingTasks > request.TotalTasks)
        {
            throw new BusinessRuleException("Completed and pending tasks cannot exceed total tasks");
        }

        var report = request.Adapt<WeeklyTaskReportDto>();
        if (report.Id == Guid.Empty)
        {
            report = report with { Id = Guid.NewGuid() };
        }

        await reports.AddAsync(report, cancellationToken);
        return report.Adapt<WeeklyTaskReportResult>();
    }
}
