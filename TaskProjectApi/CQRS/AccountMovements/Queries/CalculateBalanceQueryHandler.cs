using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using TaskListProject.Infrastructure.Data;
using TaskProject.Domain.Entities;
using TaskReportApi.Models;

namespace TaskReportApi.CQRS.AccountMovements.Queries;

public class CalculateBalanceQueryHandler : IRequestHandler<CalculateBalanceQuery, AccountBalanceModel>
{
    private readonly TaskContext _context;
    private readonly ILogger<CalculateBalanceQueryHandler> _logger;

    public CalculateBalanceQueryHandler(TaskContext context, ILogger<CalculateBalanceQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AccountBalanceModel> Handle(CalculateBalanceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var movements = await _context.AccountMovements
                .Where(m => m.UserId == request.UserId)
                .ToListAsync(cancellationToken);

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
            return new AccountBalanceModel
            {
                Balance = 0,
                UserId = request.UserId
            };
        }
    }
}
