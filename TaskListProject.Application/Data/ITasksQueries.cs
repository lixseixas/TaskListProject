using TaskProject.Domain.Entities;

namespace TaskListProject.Application.Data;

public interface ITasksQueries
{
    Task<List<TaskDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<TaskDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<SummarizedTasksDto>> GetSummarizedAsync(
        DateTime initialDate, DateTime finalDate, CancellationToken cancellationToken);
    Task<bool> IsTimeAvailableAsync(
        Guid id, DateTime date, DateTime initial, DateTime final, CancellationToken cancellationToken);
}
