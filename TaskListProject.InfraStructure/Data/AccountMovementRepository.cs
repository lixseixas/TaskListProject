using Microsoft.EntityFrameworkCore;
using TaskListProject.Application.Data;
using TaskProject.Domain.Entities;

namespace TaskListProject.Infrastructure.Data;

public sealed class AccountMovementRepository(TaskContext context) : IAccountMovementRepository
{
    public Task<decimal> GetBalanceAsync(Guid userId, CancellationToken cancellationToken)
    {
        return context.AccountMovements
            .Where(m => m.UserId == userId)
            .SumAsync(m => m.Amount, cancellationToken);
    }

    public Task<List<AccountMovementDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return context.AccountMovements.Where(m => m.UserId == userId).ToListAsync(cancellationToken);
    }

    public async Task<(List<AccountMovementDto> Movements, int TotalCount)> GetPageAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = context.AccountMovements.Where(m => m.UserId == userId).OrderByDescending(m => m.Date);
        var totalCount = await query.CountAsync(cancellationToken);
        var movements = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (movements, totalCount);
    }

    public async Task AddAsync(AccountMovementDto movement, CancellationToken cancellationToken)
    {
        context.AccountMovements.Add(movement);
        await context.SaveChangesAsync(cancellationToken);
    }
}
