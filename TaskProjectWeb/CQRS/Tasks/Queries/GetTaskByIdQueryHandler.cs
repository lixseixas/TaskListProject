using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using TaskListProject.Infrastructure.Data;
using TaskProject.Models;
using Mapster;

namespace TaskProject.CQRS.Tasks.Queries;

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskModel>
{
    private readonly TaskContext _context;
    private readonly ILogger<GetTaskByIdQueryHandler> _logger;

    public GetTaskByIdQueryHandler(TaskContext context, ILogger<GetTaskByIdQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TaskModel> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var task = await _context.Tasks.FindAsync(new object[] { request.Id }, cancellationToken);
            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found", request.Id);
                return new TaskModel();
            }

            return task.Adapt<TaskModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task with ID {TaskId}", request.Id);
            return new TaskModel();
        }
    }
}
