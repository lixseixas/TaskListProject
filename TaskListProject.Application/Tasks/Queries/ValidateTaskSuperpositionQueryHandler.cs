using MediatR;
using TaskListProject.Application.Data;

namespace TaskListProject.Application.Tasks.Queries;

public sealed class ValidateTaskSuperpositionQueryHandler(ITasksQueries tasks)
    : IRequestHandler<ValidateTaskSuperpositionQuery, bool>
{
    public Task<bool> Handle(ValidateTaskSuperpositionQuery request, CancellationToken cancellationToken)
    {
        return tasks.IsTimeAvailableAsync(
            request.Id, request.Date, request.Initial, request.Final, cancellationToken);
    }
}
