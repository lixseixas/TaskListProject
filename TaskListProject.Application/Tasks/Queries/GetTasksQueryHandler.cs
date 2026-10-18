using Mapster;
using MediatR;
using TaskListProject.Application.Data;

namespace TaskListProject.Application.Tasks.Queries;

public sealed class GetTasksQueryHandler(ITasksQueries tasks) : IRequestHandler<GetTasksQuery, List<TaskResult>>
{
    public async Task<List<TaskResult>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var result = await tasks.GetAllAsync(cancellationToken);
        return result.Adapt<List<TaskResult>>();
    }
}
