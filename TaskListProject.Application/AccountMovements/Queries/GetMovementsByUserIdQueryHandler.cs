using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using TaskListProject.Application.Data;
using TaskProject.Domain.Entities;
using Mapster;

namespace TaskListProject.Application.AccountMovements.Queries;

public class GetMovementsByUserIdQueryHandler : IRequestHandler<GetMovementsByUserIdQuery, AccountMovementsResult>
{
    private readonly IAccountMovementRepository _movements;
    private readonly ILogger<GetMovementsByUserIdQueryHandler> _logger;

    public GetMovementsByUserIdQueryHandler(IAccountMovementRepository movements, ILogger<GetMovementsByUserIdQueryHandler> logger)
    {
        _movements = movements;
        _logger = logger;
    }

    public async Task<AccountMovementsResult> Handle(GetMovementsByUserIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (movements, totalCount) = await _movements.GetPageAsync(
                request.UserId, request.Page, request.PageSize, cancellationToken);

            var movementModels = movements.Adapt<List<AccountMovementResult>>();

            return new AccountMovementsResult
            {
                Movements = movementModels,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving movements for UserId {UserId}", request.UserId);
            throw;
        }
    }
}
