using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TaskListProject.Infrastructure.Data;
using TaskProject.Domain.Entities;
using TaskProject.Models;
using Mapster;

namespace TaskProject.CQRS.Tasks.Commands;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, bool>
{
    private readonly TaskContext _context;
    private readonly ILogger<UpdateTaskCommandHandler> _logger;

    public UpdateTaskCommandHandler(TaskContext context, ILogger<UpdateTaskCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var taskDto = request.Task.Adapt<TaskDto>();
            
            var existingTask = await _context.Tasks.FindAsync(new object[] { taskDto.Id }, cancellationToken);
            if (existingTask == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found", taskDto.Id);
                return false;
            }

            existingTask.Title = taskDto.Title;
            existingTask.Description = taskDto.Description;
            existingTask.Date = taskDto.Date;
            existingTask.InitialHour = taskDto.InitialHour;
            existingTask.FinalHour = taskDto.FinalHour;
            existingTask.Priority = taskDto.Priority;
            existingTask.Ended = taskDto.Ended;

            await _context.SaveChangesAsync(cancellationToken);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task with ID {TaskId}", request.Task.Id);
            return false;
        }
    }
}
