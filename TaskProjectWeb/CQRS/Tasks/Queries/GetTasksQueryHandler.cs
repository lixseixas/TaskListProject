using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskListProject.Infrastructure.Data;
using TaskProject.Models;
using Mapster;

namespace TaskProject.CQRS.Tasks.Queries;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, TaskListModel>
{
    private readonly TaskContext _context;
    private readonly ILogger<GetTasksQueryHandler> _logger;

    public GetTasksQueryHandler(TaskContext context, ILogger<GetTasksQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TaskListModel> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tasks = await _context.Tasks.ToListAsync(cancellationToken);
            var taskModels = tasks.Adapt<List<TaskModel>>();

            return new TaskListModel { TaskList = taskModels };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks");
            return new TaskListModel { TaskList = new List<TaskModel>() };
        }
    }
}
