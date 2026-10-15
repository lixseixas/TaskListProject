using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using TaskListProject.Application.Data;
using TaskProject.Domain.Entities;
using TaskReportApi.Models;
using Mapster;

namespace TaskReportApi.CQRS.AccountMovements.Queries;

public class GetMovementsByUserIdQueryHandler : IRequestHandler<GetMovementsByUserIdQuery, AccountMovementsResponseModel>
{
    private readonly IAccountMovementRepository _movements;
    private readonly ILogger<GetMovementsByUserIdQueryHandler> _logger;

    public GetMovementsByUserIdQueryHandler(IAccountMovementRepository movements, ILogger<GetMovementsByUserIdQueryHandler> logger)
    {
        _movements = movements;
        _logger = logger;
    }

    public async Task<AccountMovementsResponseModel> Handle(GetMovementsByUserIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (movements, totalCount) = await _movements.GetPageAsync(
                request.UserId, request.Page, request.PageSize, cancellationToken);

            var movementModels = movements.Adapt<List<AccountMovementModel>>();

            return new AccountMovementsResponseModel
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
