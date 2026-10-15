using TaskProject.Domain.Entities;

namespace TaskListProject.Application.Data;

public interface IAccountMovementRepository
{
    Task<decimal> GetBalanceAsync(Guid userId, CancellationToken cancellationToken);
    Task<List<AccountMovementDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<(List<AccountMovementDto> Movements, int TotalCount)> GetPageAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken);
    Task AddAsync(AccountMovementDto movement, CancellationToken cancellationToken);
}
