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

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, bool>
{
    private readonly TaskContext _context;
    private readonly ILogger<CreateTaskCommandHandler> _logger;

    public CreateTaskCommandHandler(TaskContext context, ILogger<CreateTaskCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var taskDto = request.Task.Adapt<TaskDto>();
            taskDto.Id = Guid.NewGuid();
            
            _context.Tasks.Add(taskDto);
            await _context.SaveChangesAsync(cancellationToken);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return false;
        }
    }
}
