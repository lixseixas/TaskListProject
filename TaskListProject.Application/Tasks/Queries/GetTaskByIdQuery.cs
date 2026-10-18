using MediatR;

namespace TaskListProject.Application.Tasks.Queries;

public sealed record GetTaskByIdQuery(Guid Id) : IRequest<TaskResult?>;
