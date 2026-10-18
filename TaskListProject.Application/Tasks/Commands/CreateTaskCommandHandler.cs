using Mapster;
using MediatR;
using TaskListProject.Application.Data;
using TaskProject.Domain.Entities;

namespace TaskListProject.Application.Tasks.Commands;

public sealed class CreateTaskCommandHandler(ITasksCommands tasks) : IRequestHandler<CreateTaskCommand, TaskResult>
{
    public async Task<TaskResult> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = request.Adapt<TaskDto>();
        if (task.Id == Guid.Empty)
        {
            task.Id = Guid.NewGuid();
        }

        await tasks.AddAsync(task, cancellationToken);
        return task.Adapt<TaskResult>();
    }
}
