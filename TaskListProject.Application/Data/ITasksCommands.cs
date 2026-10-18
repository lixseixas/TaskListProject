using TaskProject.Domain.Entities;

namespace TaskListProject.Application.Data;

public interface ITasksCommands
{
    Task AddAsync(TaskDto task, CancellationToken cancellationToken);
    Task<TaskDto?> UpdateAsync(TaskDto task, CancellationToken cancellationToken);
}
