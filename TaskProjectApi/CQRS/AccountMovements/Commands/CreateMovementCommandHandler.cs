using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskListProject.Infrastructure.Data;
using TaskProject.Domain.Entities;
using TaskReportApi.Models;
using Mapster;

namespace TaskReportApi.CQRS.AccountMovements.Commands;

public class CreateMovementCommandHandler : IRequestHandler<CreateMovementCommand, AccountMovementModel>
{
    private readonly TaskContext _context;
    private readonly ILogger<CreateMovementCommandHandler> _logger;

    public CreateMovementCommandHandler(TaskContext context, ILogger<CreateMovementCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AccountMovementModel> Handle(CreateMovementCommand request, CancellationToken cancellationToken)
    {
        try
        {
            //get current balance
            var currentBalance = await _context.AccountMovements
                .Where(m => m.UserId == request.UserId)
                .SumAsync(m => m.Amount, cancellationToken);

            //calculate if the new balance could be less than 0
            var newBalance = currentBalance + request.Amount;
            if (newBalance < 0)
            {
                throw new InvalidOperationException("Balance could not be less than 0");
            }

            var movement = new AccountMovementDto
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Amount = request.Amount,
                Type = request.Type,
                Date = request.Date,
                Description = request.Description
            };

            _context.AccountMovements.Add(movement);
            await _context.SaveChangesAsync(cancellationToken);

            var movementModel = movement.Adapt<AccountMovementModel>();
            
            _logger.LogInformation("Created movement with Id {MovementId} for UserId {UserId}", movement.Id, request.UserId);
            
            return movementModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating movement for UserId {UserId}", request.UserId);
            throw;
        }
    }
}
