using MediatR;

namespace TaskListProject.Application.Tasks.Queries;

public sealed record ValidateTaskSuperpositionQuery(
    Guid Id, DateTime Date, DateTime Initial, DateTime Final) : IRequest<bool>;
