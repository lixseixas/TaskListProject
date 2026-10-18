using MediatR;
using Microsoft.Extensions.Logging;
using TaskListProject.Application.Data;
using TaskProject.Domain.Entities;
using TaskListProject.Application.Common;
using Mapster;

namespace TaskListProject.Application.AccountMovements.Commands;

public class CreateMovementCommandHandler : IRequestHandler<CreateMovementCommand, AccountMovementResult>
{
    private readonly IAccountMovementRepository _movements;
    private readonly ILogger<CreateMovementCommandHandler> _logger;

    public CreateMovementCommandHandler(IAccountMovementRepository movements, ILogger<CreateMovementCommandHandler> logger)
    {
        _movements = movements;
        _logger = logger;
    }

    public async Task<AccountMovementResult> Handle(CreateMovementCommand request, CancellationToken cancellationToken)
    {
        try
        {
            //get current balance
            var currentBalance = await _movements.GetBalanceAsync(request.UserId, cancellationToken);

            //calculate if the new balance could be less than 0
            var newBalance = currentBalance + request.Amount;
            if (newBalance < 0)
            {
                throw new BusinessRuleException("Balance could not be less than 0");
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

            await _movements.AddAsync(movement, cancellationToken);

            var movementModel = movement.Adapt<AccountMovementResult>();
            
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
