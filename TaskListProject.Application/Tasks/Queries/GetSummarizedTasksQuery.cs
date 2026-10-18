using MediatR;

namespace TaskListProject.Application.Tasks.Queries;

public sealed record GetSummarizedTasksQuery : IRequest<TaskSummaryResult>
{
    public DateTime InitialDate { get; init; }
    public DateTime FinalDate { get; init; }
}
