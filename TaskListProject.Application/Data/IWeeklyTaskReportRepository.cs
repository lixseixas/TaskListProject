using TaskProject.Domain.Entities;

namespace TaskListProject.Application.Data;

public interface IWeeklyTaskReportRepository
{
    Task<List<WeeklyTaskReportDto>> GetAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
    Task AddAsync(WeeklyTaskReportDto report, CancellationToken cancellationToken);
}
