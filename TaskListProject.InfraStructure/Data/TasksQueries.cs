using Microsoft.EntityFrameworkCore;
using TaskListProject.Application.Data;
using TaskProject.Domain.Entities;

namespace TaskListProject.Infrastructure.Data;

public sealed class TasksQueries(TaskContext context) : ITasksQueries
{
    public async Task<List<TaskDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var tasks = await context.Tasks.AsNoTracking().OrderBy(task => task.Date).ToListAsync(cancellationToken);
        foreach (var task in tasks)
        {
            task.PriorityName = task.Priority switch
            {
                3 => "High",
                2 => "Medium",
                1 => "Low",
                _ => string.Empty
            };
        }

        return tasks;
    }

    public Task<TaskDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Tasks.AsNoTracking().SingleOrDefaultAsync(task => task.Id == id, cancellationToken);
    }

    public async Task<List<SummarizedTasksDto>> GetSummarizedAsync(
        DateTime initialDate, DateTime finalDate, CancellationToken cancellationToken)
    {
        var end = finalDate.AddHours(23).AddMinutes(59);
        var tasks = await context.Tasks.AsNoTracking()
            .Where(task => task.Date >= initialDate && task.Date <= end)
            .OrderBy(task => task.Date)
            .ToListAsync(cancellationToken);
        var result = new List<SummarizedTasksDto>();

        foreach (var day in tasks.GroupBy(task => task.Date))
        {
            var totalHours = TimeSpan.Zero;
            var completedHours = TimeSpan.Zero;
            foreach (var task in day)
            {
                var hours = DateTime.Parse(task.FinalHour.ToShortTimeString())
                    - DateTime.Parse(task.InitialHour.ToShortTimeString());
                totalHours += hours;
                if (task.Ended)
                {
                    completedHours += hours;
                }
            }

            result.Add(new SummarizedTasksDto
            {
                Date = day.Key,
                Hours = totalHours.ToString(),
                TotalTasks = day.Count(),
                AverageHours = (totalHours / day.Count()).ToString(),
                PercentualConcludedTasks = Math.Round(completedHours.TotalMinutes / totalHours.TotalMinutes * 100)
            });
        }

        return result;
    }

    public async Task<bool> IsTimeAvailableAsync(
        Guid id, DateTime date, DateTime initial, DateTime final, CancellationToken cancellationToken)
    {
        // Preserve the existing date and inclusive-boundary rules; scheduling changes belong to TD-07.
        return !await context.Tasks.AnyAsync(task =>
            task.Date >= date && task.Id != id &&
            ((task.InitialHour <= initial && task.FinalHour >= initial) ||
             (task.InitialHour <= final && task.FinalHour >= final) ||
             (task.InitialHour <= initial && task.FinalHour >= final) ||
             (task.InitialHour >= initial && task.FinalHour <= final)), cancellationToken);
    }
}
