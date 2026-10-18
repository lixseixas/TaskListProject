using Mapster;
using MediatR;
using TaskListProject.Application.Data;
using TaskProject.Domain.Entities;

namespace TaskListProject.Application.Tasks.Commands;

public sealed class UpdateTaskCommandHandler(ITasksCommands tasks) : IRequestHandler<UpdateTaskCommand, TaskResult?>
{
    public async Task<TaskResult?> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var updated = await tasks.UpdateAsync(request.Adapt<TaskDto>(), cancellationToken);
        return updated?.Adapt<TaskResult>();
    }
}
