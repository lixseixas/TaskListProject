using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using TaskListProject.Application.Data;
using TaskProject.Domain.Entities;
using TaskReportApi.Models;

namespace TaskReportApi.CQRS.AccountMovements.Queries;

public class CalculateBalanceQueryHandler : IRequestHandler<CalculateBalanceQuery, AccountBalanceModel>
{
    private readonly IAccountMovementRepository _movements;
    private readonly ILogger<CalculateBalanceQueryHandler> _logger;

    public CalculateBalanceQueryHandler(IAccountMovementRepository movements, ILogger<CalculateBalanceQueryHandler> logger)
    {
        _movements = movements;
        _logger = logger;
    }

    public async Task<AccountBalanceModel> Handle(CalculateBalanceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var movements = await _movements.GetByUserIdAsync(request.UserId, cancellationToken);

            decimal balance = 0;

            foreach (var movement in movements)
            {
                balance += movement.Amount;
            }

            // Limit the account balance to 0
            balance = Math.Max(0, balance);

            return new AccountBalanceModel
            {
                Balance = balance,
                UserId = request.UserId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating balance for UserId {UserId}", request.UserId);
            throw;
        }
    }
}
