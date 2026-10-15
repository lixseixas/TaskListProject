using Microsoft.EntityFrameworkCore;
using TaskListProject.Application.Data;
using TaskProject.Domain.Entities;

namespace TaskListProject.Infrastructure.Data;

public sealed class WeeklyTaskReportRepository(TaskContext context) : IWeeklyTaskReportRepository
{
    public Task<List<WeeklyTaskReportDto>> GetAsync(
        DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        return context.WeeklyTaskReports
            .Where(t => t.WeekStartDate >= startDate && t.WeekEndDate <= endDate)
            .OrderBy(t => t.WeekStartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(WeeklyTaskReportDto report, CancellationToken cancellationToken)
    {
        context.WeeklyTaskReports.Add(report);
        await context.SaveChangesAsync(cancellationToken);
    }
}
