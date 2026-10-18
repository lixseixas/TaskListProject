using Mapster;
using MediatR;
using TaskListProject.Application.Data;

namespace TaskListProject.Application.Tasks.Queries;

public sealed class GetSummarizedTasksQueryHandler(ITasksQueries tasks)
    : IRequestHandler<GetSummarizedTasksQuery, TaskSummaryResult>
{
    public async Task<TaskSummaryResult> Handle(GetSummarizedTasksQuery request, CancellationToken cancellationToken)
    {
        var summaries = await tasks.GetSummarizedAsync(request.InitialDate, request.FinalDate, cancellationToken);
        return new TaskSummaryResult(
            request.InitialDate, request.FinalDate, summaries.Adapt<List<SummarizedTasksResult>>());
    }
}
