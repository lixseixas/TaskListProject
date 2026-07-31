using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskListProject.Infrastructure.Data;
using TaskProject.Models;
using Mapster;

namespace TaskProject.CQRS.Tasks.Queries;

public class GetSummarizedTasksQueryHandler : IRequestHandler<GetSummarizedTasksQuery, SearchTaskModel>
{
    private readonly TasksQueries _tasksQueries;
    private readonly ILogger<GetSummarizedTasksQueryHandler> _logger;

    public GetSummarizedTasksQueryHandler(TasksQueries tasksQueries, ILogger<GetSummarizedTasksQueryHandler> logger)
    {
        _tasksQueries = tasksQueries;
        _logger = logger;
    }

    public async Task<SearchTaskModel> Handle(GetSummarizedTasksQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var summarizedDtos = new List<TaskProject.Domain.Entities.SummarizedTasksDto>();
            var success = _tasksQueries.GetSummarizedTasks(request.InitialDate, request.FinalDate, ref summarizedDtos);

            if (!success)
            {
                _logger.LogWarning("Failed to retrieve summarized tasks for date range {StartDate} to {EndDate}", 
                    request.InitialDate, request.FinalDate);
                return new SearchTaskModel();
            }

            var summarizedModels = summarizedDtos.Adapt<List<SummarizedTasksModel>>();

            return new SearchTaskModel
            {
                InitialDate = request.InitialDate,
                FinalDate = request.FinalDate,
                ListTasksSummarized = summarizedModels
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving summarized tasks for date range {StartDate} to {EndDate}", 
                request.InitialDate, request.FinalDate);
            return new SearchTaskModel();
        }
    }
}
