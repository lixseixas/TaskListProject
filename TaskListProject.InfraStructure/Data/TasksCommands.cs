using Microsoft.EntityFrameworkCore;
using TaskListProject.Application.Data;
using TaskProject.Domain.Entities;

namespace TaskListProject.Infrastructure.Data;

public sealed class TasksCommands(TaskContext context) : ITasksCommands
{
    public async Task AddAsync(TaskDto task, CancellationToken cancellationToken)
    {
        context.Tasks.Add(task);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TaskDto?> UpdateAsync(TaskDto task, CancellationToken cancellationToken)
    {
        var existing = await context.Tasks.SingleOrDefaultAsync(item => item.Id == task.Id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.Title = task.Title;
        existing.Description = task.Description;
        existing.Date = task.Date;
        existing.InitialHour = task.InitialHour;
        existing.FinalHour = task.FinalHour;
        existing.Priority = task.Priority;
        existing.Ended = task.Ended;
        await context.SaveChangesAsync(cancellationToken);
        return existing;
    }
}
