using MediatR;

namespace TaskListProject.Application.Tasks.Queries;

public sealed record GetTasksQuery : IRequest<List<TaskResult>>;
