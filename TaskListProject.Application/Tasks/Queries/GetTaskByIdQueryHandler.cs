using Mapster;
using MediatR;
using TaskListProject.Application.Data;

namespace TaskListProject.Application.Tasks.Queries;

public sealed class GetTaskByIdQueryHandler(ITasksQueries tasks) : IRequestHandler<GetTaskByIdQuery, TaskResult?>
{
    public async Task<TaskResult?> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetByIdAsync(request.Id, cancellationToken);
        return task?.Adapt<TaskResult>();
    }
}
