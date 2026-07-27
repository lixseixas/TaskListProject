using TaskProject.Models;
using System.Threading;
using System.Threading.Tasks;

namespace TaskProject.Services;

public interface IWeeklyTaskReportPublisher
{
    Task PublishAsync(WeeklyTaskReportModel report, CancellationToken cancellationToken = default);
}
